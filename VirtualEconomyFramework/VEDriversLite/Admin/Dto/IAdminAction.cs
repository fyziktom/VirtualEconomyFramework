using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    public enum AdminActionTypes
    {
        Common,
        AddNeblioAccount,
        ImportVENFTBackup,
        AddDogeAccount,
    }
    public interface IAdminAction
    {
        string Admin { get; set; }
        string Message { get; set; }
        string Signature { get; set; }
        string Data { get; set; }
        AdminActionTypes Type { get; set; }
        string CreateNewMessage();
    }
}
