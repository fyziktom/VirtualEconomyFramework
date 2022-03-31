using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    /// <summary>
    /// Import basic account Backup Dto
    /// </summary>
    public class ImportBackupDto
    {
        /// <summary>
        /// Admin credentials info
        /// Include Admin Address, Message and Signature of this message
        /// </summary>
        public AdminActionBase adminCredentials { get; set; } = new AdminActionBase();

        /// <summary>
        /// Data dto
        /// </summary>
        public EncryptedBackupDto dto { get; set; } = new EncryptedBackupDto();
    }
}
