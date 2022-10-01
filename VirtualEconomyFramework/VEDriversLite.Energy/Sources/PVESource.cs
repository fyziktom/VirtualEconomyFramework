using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.Energy.Sources
{
    public class PVESource : CommonSource
    {
        public PVESource()
        {
            Type = EntityType.Source;
            SourceType = SourceType.PVE;
        }

        public double AngleOfPanels { get; set; } = 0.0;
        public double MaximumDischargePower { get; set; } = 0.0;

    }
}
