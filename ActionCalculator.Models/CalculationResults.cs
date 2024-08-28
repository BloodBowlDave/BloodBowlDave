namespace ActionCalculator.Models;

public class CalculationResults
{
    public CalculationResults(List<CalculationResult> results)
    {
        Results = results;
        Exceptions = new List<string>();
    }
    
    public CalculationResults(List<string> exceptions)
    {
        Exceptions = exceptions;
        Results = new List<CalculationResult>();
    }
    
    public List<CalculationResult> Results { get; set; }
    public List<string> Exceptions { get; set; }
    public bool CalculationIsValid => !Exceptions.Any();
}
