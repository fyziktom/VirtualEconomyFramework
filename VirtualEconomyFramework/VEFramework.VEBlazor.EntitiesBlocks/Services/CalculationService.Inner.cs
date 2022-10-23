namespace VEFramework.VEBlazor.EntitiesBlocks.Services;

public partial class CalculationService
{
    private Task DoCalculation(string filteredEntitiesConfig, string filteredPveConfig, string filteredStorageConfig,
        decimal budget, decimal interestRate, DateTime endDate, IDictionary<string, bool> deviceLeadingMap)
    {
        Console.WriteLine("Running calculation");
        return Task.CompletedTask;
    }
}