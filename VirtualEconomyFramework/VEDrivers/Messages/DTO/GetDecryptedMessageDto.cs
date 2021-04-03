using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Messages.DTO
{
    /// <summary>
    /// Data carrier for decrypt message API command
    /// </summary>
    public class GetDecryptedMessageDto
    {
        /// <summary>
        /// Guid format of wallet Id
        /// </summary>
        public string walletId { get; set; }
        /// <summary>
        /// Account address, now just Neblio addresses
        /// </summary>
        public string accountAddress { get; set; }
        /// <summary>
        /// Key which can be matched in the metadata field
        /// </summary>
        public string SenderPubKey { get; set; }
        /// <summary>
        /// password to unlock the key from storage
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Message txid
        /// </summary>
        public string TxId { get; set; }
    }
}
