using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// Dto for the export and import of the account or subaccount
    /// </summary>
    public class AccountExportDto
    {
        /// <summary>
        /// Address of the account
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Encrypted key
        /// Usually the subaccount are encrypted with the master account key
        /// </summary>
        public string EKey { get; set; } = string.Empty;
        /// <summary>
        /// If the key is encrypted with use of the Symetric encryption load it here
        /// </summary>
        public string ESKey { get; set; } = string.Empty;
        /// <summary>
        /// Name of the account or subaccount
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Password for decryption of the EKey
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Set this flag if it is doge account
        /// </summary>
        public bool IsDogeAccount { get; set; } = false;
        /// <summary>
        /// Connect this account to the Main shop account
        /// </summary>
        public bool ConnectToMainShop { get; set; } = false;
        /// <summary>
        /// Set this if this is receiving account of the use of the shop functions
        /// </summary>
        public bool IsReceivingAccount { get; set; } = false;
        /// <summary>
        /// Set this if this is deposit account of the use of the shop functions
        /// </summary>
        public bool IsDepositAccount { get; set; } = false;
    }
}
