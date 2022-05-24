using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common
{
    /// <summary>
    /// Configuration of the RPC node connection
    /// </summary>
    public class QTRPCConfig
    {
        /// <summary>
        /// Host of RPC server
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";
        /// <summary>
        /// Port of RPC server
        /// </summary>
        public int Port { get; set; } = 6326;
        /// <summary>
        /// User name for RPC server
        /// </summary>
        public string User { get; set; } = "user";
        /// <summary>
        /// Password for RPC server
        /// </summary>
        public string Pass { get; set; } = "userpass";
    }
}
