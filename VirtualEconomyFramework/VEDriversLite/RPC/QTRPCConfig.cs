using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common
{
    public class QTRPCConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 6326;
        public string User { get; set; } = "user";
        public string Pass { get; set; } = "userpass";
    }
}
