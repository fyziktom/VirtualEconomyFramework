using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    public class ConnectDogeAccountDto
    {
        /// <summary>
        /// Admin credentials info
        /// Include Admin Address, Message and Signature of this message
        /// </summary>
        public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
        public string dogeAccountAddress { get; set; } = string.Empty;
        public string AccountAddress { get; set; } = string.Empty;
    }
}
