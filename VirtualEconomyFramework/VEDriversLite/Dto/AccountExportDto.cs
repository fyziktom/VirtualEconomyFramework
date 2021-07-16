using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    public class AccountExportDto
    {
        public string Address { get; set; } = string.Empty;
        public string EKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsDogeAccount { get; set; } = false;
        public bool ConnectToMainShop { get; set; } = false;
    }
}
