using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    /// <summary>
    /// Dto for request of write data stream to the storage
    /// </summary>
    public class WriteStreamRequestDto
    {
        /// <summary>
        /// File Name
        /// </summary>
        public string? Filename { get; set; }
        /// <summary>
        /// Stream of the data
        /// </summary>
        public Stream? Data { get; set; }
        /// <summary>
        /// Set if you want to create backup of local file
        /// </summary>
        public bool BackupInLocal { get; set; }
        /// <summary>
        /// Prefered storage ID
        /// </summary>
        public string? StorageId { get; set; }
        /// <summary>
        /// Storage driver Type
        /// </summary>
        public StorageDriverType DriverType { get; set; }
    }
}
