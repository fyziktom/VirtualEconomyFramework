using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public enum TransactionTypes
    {
        Common,
        Bitcoin,
        Neblio
    }
    public enum TransactionDirection
    {
        Incoming,
        Outcoming
    }
    public interface ITransaction
    {
        TransactionTypes Type { get; set; }
        TransactionDirection Direction { get; set; }
        string TxId { get; set; }
        List<string> From { get; set; }
        List<string> To { get; set; }
        
        double Ammount { get; set; }
        int Confirmations { get; set; }
        DateTime TimeStamp { get; set; }
        //ICryptocurrency Currency { get; set; }
        List<IToken> VinTokens { get; set; }
        List<IToken> VoutTokens { get; set; }
        ConcurrentDictionary<string, string> Metadata { get; set; }
    }
}
