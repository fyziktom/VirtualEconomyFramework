using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Nodes.Dto
{
    public class JSResultDto
    {
        // if this is false, the Node action 
        public bool done { get; set; } = false;
        // custom payload string 
        public string payload { get; set; } = null;
    }
}
