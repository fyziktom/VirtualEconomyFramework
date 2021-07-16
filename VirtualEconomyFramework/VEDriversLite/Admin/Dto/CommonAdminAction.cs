using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    public class AdminActionBase
    {
        public string Admin { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }
    public class CommonAdminAction : AdminActionBase, IAdminAction
    {
        public string Data { get; set; } = string.Empty;
        public AdminActionTypes Type { get; set; } = AdminActionTypes.Common;

        public virtual string CreateNewMessage()
        {
            var time = DateTime.UtcNow.ToString("dd MM yyyy hh:mm");
            var combo = Admin + time + Guid.NewGuid().ToString();
            Message = Security.SecurityUtils.ComputeSha256Hash(combo);
            return Message;
        }
    }
}
