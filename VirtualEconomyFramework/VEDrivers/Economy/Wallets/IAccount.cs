using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.Coins;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Nodes;

namespace VEDrivers.Economy.Wallets
{
    public enum AccountTypes
    {
        Bitcoin,
        Neblio,
        ReddCoin
    }

    public interface IAccount : IBalance, ICommonDbObjectBase
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        Guid WalletId { get; set; }
        AccountTypes Type { get; set; }
        Guid OwnerId { get; set; }
        double NumberOfTransaction { get; set; }

        IDictionary<string, IToken> Tokens { get; set; }

        ConcurrentDictionary<string, ITransaction> Transactions { get; set; }

        Task<string> GetDetails();
        
    }
}
