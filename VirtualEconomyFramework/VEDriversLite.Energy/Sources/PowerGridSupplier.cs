using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.Energy.Sources
{
    public class PowerGridSupplier : CommonSource
    {
        public PowerGridSupplier()
        {
            Type = EntityType.Source;
            SourceType = SourceType.PowerGridSupplier;
        }

    }
}
