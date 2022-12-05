namespace VEFramework.VEBlazor.EntitiesBlocks.Model;

public class CalculationResult
{
    public DateTime Date { get; set; }
    public double ConsumedFromFVE { get; set; }
    public double OverProductionFromFVE { get; set; }
    public double Deficiency { get; set; }
}