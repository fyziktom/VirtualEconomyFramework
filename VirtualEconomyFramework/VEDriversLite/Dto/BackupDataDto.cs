using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    public class BackupDataDto
    {
        public string Bookmarks { get; set; } = string.Empty;
        public string BrowserTabs { get; set; } = string.Empty;
        public string SubAccounts { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string DogeAddress { get; set; } = string.Empty;
        public string DogeKey { get; set; } = string.Empty;
    }
}
