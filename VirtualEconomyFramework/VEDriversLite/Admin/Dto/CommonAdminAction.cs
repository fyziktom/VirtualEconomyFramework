using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    /// <summary>
    /// Base parameters for Admin action class
    /// </summary>
    public class AdminActionBase
    {
        /// <summary>
        /// Neblio Admin Address
        /// </summary>
        public string Admin { get; set; } = string.Empty;
        /// <summary>
        /// Message for sign
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Signature of the message by admin address
        /// </summary>
        public string Signature { get; set; } = string.Empty;
    }
    /// <summary>
    /// Basic implementation of the Admin Action class
    /// </summary>
    public class CommonAdminAction : AdminActionBase, IAdminAction
    {
        /// <summary>
        /// Data for the command/action
        /// </summary>        
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Action type
        /// </summary>        
        public AdminActionTypes Type { get; set; } = AdminActionTypes.Common;

        /// <summary>
        /// Create new message for possibility to sign
        /// This message is sent to the admin and he must sign it and send back with command
        /// </summary>
        /// <returns></returns>        
        public virtual string CreateNewMessage()
        {
            var time = DateTime.UtcNow.ToString("dd MM yyyy hh:mm");
            var combo = Admin + time + Guid.NewGuid().ToString();
            Message = Security.SecurityUtils.ComputeSha256Hash(combo);
            return Message;
        }
    }
}
