using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Messages.DTO
{
    public class SendMessageDto
    {
        public string WalletId { get; set; } = string.Empty;
        /// <summary>
        /// UID of the communication stream
        /// </summary>
        public string MessageStreamUID { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public string ReceiverPubKey { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
        public string SenderPubKey { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;
        public string PrevTxId { get; set; } = string.Empty;
        public string TokenTxId { get; set; } = string.Empty;
        public bool InitMessage { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string MessageData { get; set; } = string.Empty;
        /// <summary>
        /// This password is for loading decryption password for decrypt previous message
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// This password is for sending transaction if the account is locked and you want to unlock it for this one tx
        /// </summary>
        public string AccountPassword { get; set; } = string.Empty;
        public bool Encrypt { get; set; } = true;
    }
}
