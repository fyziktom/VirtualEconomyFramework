using VEDriversLite.EntitiesBlocks.Entities;

namespace VEFramework.VEBlazor.EntitiesBlocks.Model;

public class CalculationEntity
{
        public string? Name { get; set; }
        public string Icon { get; set; }
        public int Depth { get; set; }
        public bool IsLeading { get; set; }
        public IEntity Entity { get; set; }
        public bool IsSelected { get; set; }
        public double ResultCostsBefore { get; set; }
        public double ResultCostsAfter { get; set; }
        public double AllocationKey { get; set; }
        public bool HasChildren { get; set; }
}