using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// Deposit scheme can split the amount to multiple accounts.
    /// </summary>
    public class DepositScheme
    {
        /// <summary>
        /// Name of the deposit scheme
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Name of the currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>
        /// Main deposit address (if there is no others the payment goes there
        /// </summary>
        public string MainDepositAddress { get; set; } = string.Empty;
        /// <summary>
        /// Is the deposit scheme active
        /// </summary>
        public bool IsActive { get; set; } = true;
        /// <summary>
        /// Dictionary of the addresses for the deposit of the received payment for some NFT
        /// If there are more then one the amount is splited based on the percentage in DepositAddress class
        /// </summary>
        public Dictionary<string, DepositAddress> DepositAddresses { get; set; } = new Dictionary<string, DepositAddress>();
    }
}
