using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.PVECalculations.Dto
{
    public class PVPanelsGroupConfigDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, PVPanel> Panels { get; set; } = new Dictionary<string, PVPanel>();
    }
    public class PVConfigDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PVPanel CommonPanel { get; set; } = new PVPanel();
        public Dictionary<string, PVPanelsGroupConfigDto> Groups { get; set; } = new Dictionary<string, PVPanelsGroupConfigDto>();
    }
}
