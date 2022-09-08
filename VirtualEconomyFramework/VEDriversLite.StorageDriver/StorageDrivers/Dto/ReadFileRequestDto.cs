using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    /// <summary>
    /// Dto for request of get data stream from the storage
    /// </summary>
    public class ReadFileRequestDto
    {
        /// <summary>
        /// Hash of the file or filename
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// Get from the local storage if it is available
        /// </summary>
        public bool GetFromLocalIfAvailable { get; set; }
        /// <summary>
        /// Prefered Storage ID
        /// </summary>
        public string? StorageId { get; set; }
        /// <summary>
        /// Driver type
        /// </summary>
        public StorageDriverType DriverType { get; set; }
    }
}
