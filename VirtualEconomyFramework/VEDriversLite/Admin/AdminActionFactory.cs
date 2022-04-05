using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Admin.Dto;

namespace VEDriversLite.Admin
{
    /// <summary>
    /// Factory for create Admin Action
    /// </summary>
    public static class AdminActionFactory
    {
        /// <summary>
        /// Get Admin action for access to the Accounts list in the VEDLDataContext.Accounts
        /// </summary>
        /// <param name="admin"></param>
        /// <param name="type"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
