using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Accounts.Dto
{
    public class Utxo
    {
        public AccountType AccountType { get; set; } = AccountType.Neblio;
        public string Address { get; set; } = string.Empty;
        public string Txid { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public int Index { get; set; } = 0;
        public double Value { get; set; } = 0.0;
        public double Blockheight { get; set; } = 0;
        public int Confirmations { get; set; } = 0;
        public double Time { get; set; } = 0.0;
        public List<Token> Tokens { get; set; } = new List<Token>();
    }
}
