using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Coins;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Nodes;

namespace VEDrivers.Economy.Wallets
{
    public class NeblioAccount : CommonAccount
    {
        public NeblioAccount()
        {
            Tokens = new ConcurrentDictionary<string, IToken>();
            Transactions = new ConcurrentDictionary<string, ITransaction>();
            NumberOfTransaction = 0;
            Type = AccountTypes.Neblio;
        }

        public void AddToken(string address, IToken token)
        {
            Tokens.Add(address, token);
        }

        public override async Task<string> GetDetails()
        {
            return await Task.FromResult("OK");
        }

    }
}
