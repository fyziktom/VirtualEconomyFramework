using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    public class StorageDriverConfigDto
    {
        public string Type { get; set; } = "None";
        public string Location { get; set; } = "Local";
        public string ID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public StorageDriverConnectionParams ConnectionParams { get; set; } = new StorageDriverConnectionParams();
        public bool IsLocal { get; set; } = false;
        public bool IsPublicGateway { get; set; } = false;
    }
}
