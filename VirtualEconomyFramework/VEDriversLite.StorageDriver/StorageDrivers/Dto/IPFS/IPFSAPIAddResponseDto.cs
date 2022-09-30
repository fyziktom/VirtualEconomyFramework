using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto.IPFS
{
    /// <summary>
    /// Dto for communication with IPFS API
    /// </summary>
    public class IPFSAPIAddResponseDto
    {
        /// <summary>
        /// Name of the file
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Hash of the file
        /// </summary>
        public string? Hash { get; set; }
    }
}
