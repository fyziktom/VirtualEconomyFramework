using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public abstract class CommonTransaction : ITransaction
    {
        public TransactionTypes Type { get; set; } = TransactionTypes.Common;
        public TransactionDirection Direction { get; set; } = TransactionDirection.Incoming;
        public string TxId { get; set; } = string.Empty;
        public List<string> From { get; set; }
        public List<string> To { get; set; }
        public double Amount { get; set; } = 0.0;
        public int Confirmations { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
        //public ICryptocurrency Currency { get; set; }
        public List<IToken> VinTokens { get; set; }
        public List<IToken> VoutTokens { get; set; }
        public ConcurrentDictionary<string, string> Metadata { get; set; }

    }
}
