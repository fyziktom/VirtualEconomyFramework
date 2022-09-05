using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    public class ConnectionStatus
    {
        /// <summary>
        /// Is possible to connect to the service
        /// </summary>
        public bool IsAvailable { get; set; }
        /// <summary>
        /// Time of the last ping command response RoundtripTime
        /// </summary>
        public long LastPingRoundtripTime { get; set; }

    }
}
