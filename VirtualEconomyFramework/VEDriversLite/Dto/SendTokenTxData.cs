using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite
{
    /// <summary>
    /// Data carrier for sending token
    /// </summary>
    public class SendTokenTxData
    {
        /// <summary>
        /// Init the metdata dictionary in constructor
        /// </summary>
        public SendTokenTxData()
        {
            Metadata = new Dictionary<string, string>();
        }
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
        /// Amount of the tokens
        /// </summary>
        public double Amount { get; set; } = 0.0;
        /// <summary>
        /// Metadata dictionary, key-value pairs
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Initial Utxo for sending transaction from if you want to specify them
        /// </summary>
        public ICollection<string> sendUtxo { get; set; } = new List<string>();

        /// <summary>
        /// If you wish to add specific neblio utxo as source for the fee
        /// </summary>
        public string NeblUtxo { get; set; } = string.Empty;
        /// <summary>
        /// If this is set and you will provide NeblUtxo, but it is not found in the list of spendable nebl utxos
        /// it will find another spendable utxo
        /// If this is not set and utxo is not found it will throw exception
        /// </summary>
        public bool SendEvenNeblUtxoNotFound { get; set; } = false;

        /// <summary>
        /// If you use RPC and NBitcoin you can preffer using RPC with set this to true
        /// </summary>
        public bool UseRPCPrimarily { get; set; } = false;
    }
}
