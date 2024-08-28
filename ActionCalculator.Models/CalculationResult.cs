namespace ActionCalculator.Models
{
    public class CalculationResult
    {
        public CalculationResult(int rerolls, decimal[] results)
        {
            Rerolls = rerolls;
            Results = results;
        }

        public CalculationResult(int rerolls)
        {
            Rerolls = rerolls;
            Results = Array.Empty<decimal>();
        }
        
        public int Rerolls { get; set; }
        public decimal[] Results { get; set; }
    }
}