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
    public class IPFSAPIPinAddResponseDto
    {
        /// <summary>
        /// List of the pinned file on the IPFS node
        /// </summary>
        public List<string>? Pins { get; set; }
    }
}
