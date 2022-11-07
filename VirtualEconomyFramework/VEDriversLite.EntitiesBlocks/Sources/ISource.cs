using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Sources
{
    public enum SourceType
    {
        None,
        PVE,
        Battery,
        PowerGridSupplier,
        CogenerationUnit,
        WatterPlant
    }
    public interface ISource : IEntity
    {
        SourceType SourceType { get; set; }

    }
}
