using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.APIs.HARDWARIO.Dto
{
    public class HWDto
    {
        public string id { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public string group_id { get; set; } = string.Empty;
        public string device_id { get; set; } = string.Empty;
        public string organization_id { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string raw { get; set; } = string.Empty;

        public HWDataDto data { get; set; } = new HWDataDto();
    }
}
