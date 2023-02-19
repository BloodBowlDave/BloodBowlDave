using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ActionCalculator.Abstractions;
using ActionCalculator.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BloodBowlDave.Api
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

        [FunctionName("Calculate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request)
        {
            try
            {
                var calculationString = request.Query["calculation"];

                if (string.IsNullOrWhiteSpace(calculationString))
                {
                    return new BadRequestObjectResult(JsonSerializer.Serialize(
                        new List<CalculationResult>
                        {
                            new(0, new List<ValidationFailure>
                            {
                                new("calculation", "Calculation string must be provided.")
                            })
                        }));
                }

                var playerActions = _playerActionsBuilder.Build(calculationString);

                if (int.TryParse(request.Query["rerolls"], out var rerolls))
                {
                    var calculationResult = _calculator.Calculate(new Calculation(playerActions, rerolls));

                    return new OkObjectResult(JsonSerializer.Serialize(new List<CalculationResult> { calculationResult }));
                }

                var calculationResults = new List<CalculationResult>
                {
                    _calculator.Calculate(new Calculation(playerActions, 0))
                };

                if (!calculationResults[0].IsValid)
                {
                    return new BadRequestObjectResult(JsonSerializer.Serialize(calculationResults));
                }

                for (var r = 1; r <= 8; r++)
                {
                    var calculationResult = _calculator.Calculate(new Calculation(playerActions, r));

                    if (!calculationResults[r - 1].Results.Equals(calculationResult.Results))
                    {
                        calculationResults.Add(calculationResult);
                    }
                }

                return new OkObjectResult(calculationResults);
            }
            catch (Exception)
            {
                return new BadRequestObjectResult(JsonSerializer.Serialize(
                    new List<CalculationResult>
                    {
                        new(0, new List<ValidationFailure>
                        {
                            new("unknown", "Unknown exception.")
                        })
                    }));
            }
        }
    }
}
