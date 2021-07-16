using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    public class EncryptedBackupDto
    {
        public string eadd { get; set; } = string.Empty;
        public string epass { get; set; } = string.Empty;
        public string edata { get; set; } = string.Empty;
        public bool asAdmin { get; set; } = true;
    }
}
