using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Admin.Dto
{
    /// <summary>
    /// Type of the Admin action which can be checked before executing the action.
    /// </summary>
    public enum AdminActionTypes
    {
        /// <summary>
        /// Basic action
        /// </summary>
        Common,
        /// <summary>
        /// Action for adding Neblio Account
        /// </summary>
        AddNeblioAccount,
        /// <summary>
        /// Action for load whole VENFT App backup file
        /// </summary>
        ImportVENFTBackup,
        /// <summary>
        /// Action for load the Dogecoin Account
        /// </summary>
        AddDogeAccount,
    }
    /// <summary>
    /// Basic interface for Admin Actions
    /// </summary>
    public interface IAdminAction
    {
        /// <summary>
        /// Neblio Admin Address
        /// </summary>
        string Admin { get; set; }
        /// <summary>
        /// Message for sign
        /// </summary>
        string Message { get; set; }
        /// <summary>
        /// Signature of the message by admin address
        /// </summary>
        string Signature { get; set; }
        /// <summary>
        /// Data for the command/action
        /// </summary>
        string Data { get; set; }
        /// <summary>
        /// Action type
        /// </summary>
        AdminActionTypes Type { get; set; }
        /// <summary>
        /// Create new message for possibility to sign
        /// This message is sent to the admin and he must sign it and send back with command
        /// </summary>
        /// <returns></returns>
        string CreateNewMessage();
    }
}
