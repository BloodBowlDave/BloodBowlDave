using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ActionCalculator.Abstractions;
using ActionCalculator.Models;
using ActionCalculator.Utilities;
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
            return new OkObjectResult(JsonSerializer.Serialize(GetCalculationResults(request)));
        }

        private CalculationResults GetCalculationResults(HttpRequest request)
        {
            try
            {
                var calculationString = request.Query["calculation"];

                if (string.IsNullOrWhiteSpace(calculationString))
                {
                    return new CalculationResults(new List<string>() { "Calculation string is null or whitespace." });
                }
                
                var playerActions = _playerActionsBuilder.Build(calculationString);

                if (!playerActions.Any())
                {
                    return new CalculationResults(new List<string>() { "Failed to build player actions." });
                }
                
                if (int.TryParse(request.Query["rerolls"], out var rerolls))
                {
                    var calculationResult = _calculator.Calculate(new Calculation(playerActions, rerolls));

                    return new CalculationResults(new List<CalculationResult>() { calculationResult });
                }

                var calculationResults = new List<CalculationResult>{ _calculator.Calculate(new Calculation(playerActions, 0)) };
                
                for (var r = 1; r <= 8; r++)
                {
                    var calculationResult = _calculator.Calculate(new Calculation(playerActions, r));

                    if (ResultsAreEqual(calculationResult, calculationResults[r - 1]))
                    {
                        break;
                    }
                    
                    calculationResults.Add(calculationResult);
                }

                return new CalculationResults(calculationResults);
            }
            catch (Exception e)
            {
                return new CalculationResults(new List<string>() { e.ToString() });
            }
        }

        private static bool ResultsAreEqual(CalculationResult a, CalculationResult b) => 
            a.Results.SequenceEqual(b.Results, new ProbabilityComparer());
    }
}
