using VEFramework.VEBlazor.EntitiesBlocks.Analytics;
using VEFramework.VEBlazor.EntitiesBlocks.Model;

namespace VEFramework.VEBlazor.EntitiesBlocks.Services;

public interface ICalculationService
{
    Task RunCalculation(IEnumerable<CalculationEntity> calculationEntities, decimal budget, decimal interestRate);
}