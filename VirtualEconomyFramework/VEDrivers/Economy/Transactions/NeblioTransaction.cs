using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public class NeblioTransaction : CommonTransaction
    {
        public NeblioTransaction(string txid)
        {
            TimeStamp = DateTime.UtcNow;
            TxId = txid;
            //Currency = currency;
            Type = TransactionTypes.Neblio;
            Direction = TransactionDirection.Incoming;
            From = new List<string>();
            To = new List<string>();
            VinTokens = new List<IToken>();
            VoutTokens = new List<IToken>();
        }
        public NeblioTransaction(string txid, List<string> from, List<string> to, ConcurrentDictionary<string,string> metadata, double ammount)
        {
            TimeStamp = DateTime.UtcNow;
            From = from;
            To = to;
            TxId = txid;
            //Currency = currency;
            Metadata = metadata;
            Ammount = ammount;
            Type = TransactionTypes.Neblio;
            VinTokens = new List<IToken>();
            VoutTokens = new List<IToken>();
        }



    }
}
