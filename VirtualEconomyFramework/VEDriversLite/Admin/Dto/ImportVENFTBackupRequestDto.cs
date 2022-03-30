using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    /// <summary>
    /// Admin Request for Import of the Backup data
    /// </summary>
    public class ImportVENFTBackupRequestDto : CommonAdminAction
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="admin"></param>
        /// <param name="address"></param>
        public ImportVENFTBackupRequestDto(string admin, string address)
        {
            Type = AdminActionTypes.ImportVENFTBackup;
            Admin = address;
            Address = address;
        }
        /// <summary>
        /// Address main address of the VENFT Backup
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// VENFT Backup data serialized to string
        /// </summary>
        public string BackupData { get; set; } = string.Empty;
    }
}
