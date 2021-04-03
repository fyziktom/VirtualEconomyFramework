using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.DTO
{
    /// <summary>
    /// Data carrier for sending token
    /// </summary>
    public class SendTokenTxData
    {
        public SendTokenTxData()
        {
            Metadata = new Dictionary<string, string>();
        }
        /// <summary>
        /// Address from where token will be send
        /// </summary>
        public string SenderAddress { get; set; }
        /// <summary>
        /// If the account is locked you can provide password directly in the send token api command
        /// if the account is unlocked or the QT wallet is connected fill empty string
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Address where token will be send
        /// </summary>
        public string ReceiverAddress { get; set; }
        /// <summary>
        /// Symbol of token
        /// </summary>
        public string Symbol { get; set; }
        /// <summary>
        /// Id of token
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Amount of the tokens
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Metadata dictionary, key-value pairs
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// If you use RPC and NBitcoin you can preffer using RPC with set this to true
        /// </summary>
        public bool UseRPCPrimarily { get; set; }
    }
}
