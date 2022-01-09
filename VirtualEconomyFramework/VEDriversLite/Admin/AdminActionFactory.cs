using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Admin.Dto;

namespace VEDriversLite.Admin
{
    public static class AdminActionFactory
    {
        public static IAdminAction GetAdminAction(string admin, AdminActionTypes type, string address)
        {
            if (string.IsNullOrEmpty(admin))
                throw new Exception("You must fill the admin.");

            switch (type)
            {
                case AdminActionTypes.ImportVENFTBackup:
                    return new ImportVENFTBackupRequestDto(admin, address);
            }
            return null;
        }
    }
}
