using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.Energy.Sources
{
    public class BatteryStorage : CommonSource
    {
        public BatteryStorage()
        {
            Type = EntityType.Source;
            SourceType = SourceType.Battery;
        }

        public double Capacity { get; set; } = 0.0;
        public double MaximumDischargePower { get; set; } = 0.0;
        public double MaximumChargePower { get; set; } = 0.0;


    }
}
