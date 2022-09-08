using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    /// <summary>
    /// Dto for configuration file with storage drivers
    /// </summary>
    public class StorageDriverConfigDto
    {
        /// <summary>
        /// Type of the driver based on enum StorageDriverType
        /// </summary>
        public string Type { get; set; } = "None";
        /// <summary>
        /// Tzpe of the location of the storage based on the LocationType
        /// </summary>
        public string Location { get; set; } = "Local";
        /// <summary>
        /// Unique ID of the driver
        /// </summary>
        public string ID { get; set; } = string.Empty;
        /// <summary>
        /// Name of the driver
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Connection parameters such as URL, IP, etc.
        /// </summary>
        public StorageDriverConnectionParams ConnectionParams { get; set; } = new StorageDriverConnectionParams();
        /// <summary>
        /// If the Driver for the Local instance of the storage, this is set
        /// </summary>
        public bool IsLocal { get; set; } = false;
        /// <summary>
        /// The information about setup of the public gateway parameters
        /// </summary>
        public bool IsPublicGateway { get; set; } = false;
    }
}
