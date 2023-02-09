using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers.Dto
{
    /// <summary>
    /// Response Dto from request of the data stream from the Storage
    /// </summary>
    public class StreamResponseDto
    {
        /// <summary>
        /// File name if available
        /// </summary>
        public string? Filename { get; set; }
        /// <summary>
        /// Data stream
        /// </summary>
        public Stream? Data { get; set; }
    }
}
