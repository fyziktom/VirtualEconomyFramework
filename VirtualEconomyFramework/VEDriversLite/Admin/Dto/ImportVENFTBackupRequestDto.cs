using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    public class ImportVENFTBackupRequestDto : CommonAdminAction
    {
        public ImportVENFTBackupRequestDto(string admin, string address)
        {
            Type = AdminActionTypes.ImportVENFTBackup;
            Admin = address;
            Address = address;
        }
        public string Address { get; set; } = string.Empty;
        public string BackupData { get; set; } = string.Empty;
    }
}
