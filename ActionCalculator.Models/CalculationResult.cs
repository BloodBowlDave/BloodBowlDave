using FluentValidation.Results;

namespace ActionCalculator.Models
{
    public class CalculationResult
    {
        public CalculationResult(int rerolls, decimal[] results)
        {
            Rerolls = rerolls;
            Results = results;
            IsValid = true;
            ValidationFailures = new List<ValidationFailure>();
        }

        public CalculationResult(int rerolls, ICollection<ValidationFailure> validationFailures)
        {
            Rerolls = rerolls;
            Results = Array.Empty<decimal>();
            IsValid = false;
            ValidationFailures = validationFailures;
        }
        
        public int Rerolls { get; set; }
        public decimal[] Results { get; set; }
        public bool IsValid { get; set; }
        public ICollection<ValidationFailure> ValidationFailures { get; set; }
    }
}