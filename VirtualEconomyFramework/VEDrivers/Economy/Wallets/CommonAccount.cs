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
    public abstract class CommonAccount : IAccount
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; } = string.Empty;
        public Guid WalletId { get; set; }
        public AccountTypes Type { get; set; }
        public Guid OwnerId { get; set; }
        public double NumberOfTransaction { get; set; } = 0;
        public double? TotalBalance { get; set; } = 0.0;
        public double? TotalSpendableBalance { get; set; } = 0.0;
        public double? TotalUnconfirmedBalance { get; set; } = 0.0;

        //Db interface interface
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Version { get; set; }
        public bool? Deleted { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }

        public IDictionary<string, IToken> Tokens { get; set; }
        public ConcurrentDictionary<string, ITransaction> Transactions { get; set; }

        public abstract Task<string> GetDetails();
    }
}
