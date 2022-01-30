using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;

namespace VEDriversLite.Accounts.Dto
{
    public enum TxToSendType
    {
        MainToken,
        Tokens,
        NFTMint,
        NFTSend,
        NFTDestroy,
        NFTMessage
    }
    public class TxToSend
    {
        public TxToSendType Type { get; set; } = TxToSendType.MainToken;
        public bool Received { get; set; } = false;
        public bool Processed { get; set; } = false;
        public bool Processing { get; set; } = false;
        public bool Failed { get; set; } = false;
        public bool Success { get; set; } = false;
        public int Attempts { get; set; } = 5;
        public string TxId { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string CustomMessage { get; set; } = string.Empty;
        public List<(double, string)> AmountsReceivers { get; set; } = new List<(double, string)>();
        public bool IsTokenTransaction { get; set; } = false;
        public string TokenId { get; set; } = string.Empty;
        public bool IsNFTTransaction { get; set; } = false;
        public List<(INFT, string)> NFTsReceivers { get; set; } = new List<(INFT, string)>();
        public List<Utxo> Utxos { get; set; } = new List<Utxo>();
    }
}
