using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    /// <summary>
    /// Minimum Dto for backup encrypted account
    /// </summary>
    public class EncryptedBackupDto
    {
        /// <summary>
        /// Address
        /// </summary>
        public string eadd { get; set; } = string.Empty;
        /// <summary>
        /// Password for Symetric encryption
        /// </summary>
        public string epass { get; set; } = string.Empty;
        /// <summary>
        /// Encrypted data
        /// </summary>
        public string edata { get; set; } = string.Empty;
        /// <summary>
        /// Load address as admin address
        /// </summary>
        public bool asAdmin { get; set; } = true;
    }
}
