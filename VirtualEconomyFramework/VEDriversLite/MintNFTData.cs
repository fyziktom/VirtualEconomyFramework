﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite
{
    public class MintNFTData
    {
        public MintNFTData()
        {
            Metadata = new Dictionary<string, string>();
        }
        /// <summary>
        /// Address from where token will be send
        /// </summary>
        public string SenderAddress { get; set; }
        /// <summary>
        /// Address from where to send new NFT
        /// </summary>
        public string ReceiverAddress { get; set; }
        /// <summary>
        /// If the account is locked you can provide password directly in the send token api command
        /// if the account is unlocked or the QT wallet is connected fill empty string
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Id of token
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Metadata dictionary, key-value pairs
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }
        /// <summary>
        /// Initial Utxo for sending transaction from
        /// </summary>
        public string sendUtxo { get; set; }

        /// <summary>
        /// If you use RPC and NBitcoin you can preffer using RPC with set this to true
        /// </summary>
        public bool UseRPCPrimarily { get; set; }
    }
}
