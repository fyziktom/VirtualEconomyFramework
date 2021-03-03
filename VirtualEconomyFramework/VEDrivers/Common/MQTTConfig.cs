using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Common
{
    public class MQTTConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 1883;
        public int WSPort { get; set; } = 8083;
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
    }
}
