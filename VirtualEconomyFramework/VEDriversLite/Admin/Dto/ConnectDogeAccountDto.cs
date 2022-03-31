using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    /// <summary>
    /// Dto for connect Doge account with Neblio Account
    /// </summary>
    public class ConnectDogeAccountDto
    {
        /// <summary>
        /// Admin credentials info
        /// Include Admin Address, Message and Signature of this message
        /// </summary>
        public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();
        /// <summary>
        /// Doge Address
        /// This Address must be already in the VEDLDataContext.DogeAccounts
        /// </summary>
        public string dogeAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// Neblio Address
        /// This Address must be already in the VEDLDataContext.Accounts
        /// </summary>
        public string AccountAddress { get; set; } = string.Empty;
    }
}
