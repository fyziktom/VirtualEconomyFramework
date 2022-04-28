using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite
{
    /// <summary>
    /// Data carrier for sending classic cryptocurrency transaction
    /// </summary>
    public class SendTxData
    {
        /// <summary>
        /// Address from where token will be send
        /// </summary>
        public string SenderAddress { get; set; } = string.Empty;
        /// <summary>
        /// If the account is locked you can provide password directly in the send token api command
        /// if the account is unlocked or the QT wallet is connected fill empty string
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Address where token will be send
        /// </summary>
        public string ReceiverAddress { get; set; } = string.Empty;
        /// <summary>
        /// Symbol of token
        /// </summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Id of token
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Amount of the currency
        /// </summary>
        public double Amount { get; set; } = 0.0;
        /// <summary>
        /// Custom message if it is supported by currency, not implemented yet
        /// </summary>
        public string CustomMessage { get; set; } = string.Empty;
    }
}
