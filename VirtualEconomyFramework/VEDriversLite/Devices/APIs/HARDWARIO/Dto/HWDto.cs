using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    /// <summary>
    /// Dto for HARDWARIO CHESTER data about hardware
    /// </summary>
    public class HWDto
    {
        /// <summary>
        /// ID of the CHESTER device message
        /// </summary>
        public string id { get; set; } = string.Empty;
        /// <summary>
        /// Message created at utc time
        /// </summary>
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Group Id where device is
        /// </summary>
        public string group_id { get; set; } = string.Empty;
        /// <summary>
        /// CHESTER Device Id
        /// </summary>
        public string device_id { get; set; } = string.Empty;
        /// <summary>
        /// Organization Id where device is
        /// </summary>
        public string organization_id { get; set; } = string.Empty;
        /// <summary>
        /// Label of the device
        /// </summary>
        public string label { get; set; } = string.Empty;
        /// <summary>
        /// Name of the device
        /// </summary>
        public string name { get; set; } = string.Empty;
        //public string raw { get; set; } = string.Empty;
        /// <summary>
        /// Message data
        /// </summary>
        public HWDataDto data { get; set; } = new HWDataDto();
    }
}
