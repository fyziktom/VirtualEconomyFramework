namespace VEFramework.VEBlazor.EntitiesBlocks.Services;

public partial class CalculationService
{
    private Task DoCalculation(string filteredEntitiesConfig, string filteredPveConfig, string filteredStorageConfig,
        decimal budget, decimal interestRate)
    {
        Console.WriteLine("Running calculation");
        return Task.CompletedTask;
    }
}