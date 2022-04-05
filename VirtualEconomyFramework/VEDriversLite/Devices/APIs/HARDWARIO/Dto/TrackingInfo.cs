using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    /// <summary>
    /// Dto for HARDWARIO CHESTER data about position and tracking
    /// </summary>
    public class TrackingInfo
    {
        /// <summary>
        /// Time of grabbed location
        /// </summary>
        public long time { get; set; } = 0;
        /// <summary>
        /// Latitude location coordinant
        /// </summary>
        public double latitude { get; set; } = 0.0;
        /// <summary>
        /// Longitude location coordinant
        /// </summary>
        public double longitude { get; set; } = 0.0;
    }
}
