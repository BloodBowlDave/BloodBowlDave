using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using ActionCalculator.Abstractions;
using ActionCalculator.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace ApiIsolated
{
    public class Calculate
    {
        private readonly ICalculator _calculator;
        private readonly IPlayerActionsBuilder _playerActionsBuilder;
        private readonly ILogger _logger;

        public Calculate(ILoggerFactory loggerFactory, ICalculator calculator, IPlayerActionsBuilder playerActionsBuilder)
        {
            _calculator = calculator;
            _playerActionsBuilder = playerActionsBuilder;
            _logger = loggerFactory.CreateLogger<Calculate>();
        }

        [Function("Calculate")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData request)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var queryString = System.Web.HttpUtility.ParseQueryString(request.Url.Query);

            var calculationString = queryString["calculation"];

            if (calculationString == null)
            {
                return null;
            }

            var playerActions = _playerActionsBuilder.Build(calculationString);

            if (int.TryParse(queryString["rerolls"], out var rerolls))
            {
                var calculationResult = _calculator.Calculate(new Calculation(playerActions, rerolls));

                var response = request.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                response.WriteString(JsonSerializer.Serialize(new List<CalculationResult> { calculationResult }));

                return response;
            }
            else
            {
                var calculationResults = new List<CalculationResult>
                {
                    _calculator.Calculate(new Calculation(playerActions, 0))
                };

                if (!calculationResults[0].IsValid)
                {
                    var badResponse = request.CreateResponse(HttpStatusCode.BadRequest);

                    badResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");

                    badResponse.WriteString(JsonSerializer.Serialize(calculationResults));

                    return badResponse;
                }

                for (var r = 1; r <= 8; r++)
                {
                    var calculationResult = _calculator.Calculate(new Calculation(playerActions, r));

                    if (!calculationResults[r - 1].Results.Equals(calculationResult.Results))
                    {
                        calculationResults.Add(calculationResult);
                    }
                }

                var response = request.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                response.WriteString(JsonSerializer.Serialize(calculationResults));

                return response;
            }
        }
    }
}
