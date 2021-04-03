using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Coins;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Nodes;
using VEDrivers.Security;

namespace VEDrivers.Economy.Wallets
{
    public abstract class CommonAccount : IAccount
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public Guid WalletId { get; set; }
        public AccountTypes Type { get; set; }
        public Guid OwnerId { get; set; }
        public double NumberOfTransaction { get; set; } = 0;
        public double NumberOfLoadedTransaction { get; } = 0;
        public double? TotalBalance { get; set; } = 0.0;
        public double? TotalSpendableBalance { get; set; } = 0.0;
        public double? TotalUnconfirmedBalance { get; set; } = 0.0;
        public bool LoadingData { get; set; } = false;
        public string SpendableTxId { get; set; } = string.Empty;
        public string LastProcessedTxId { get; set; } = string.Empty;
        public string LastConfirmedTxId { get; set; } = string.Empty;
        [JsonIgnore]
        public EncryptionKey AccountKey { get; set; }
        [JsonIgnore]
        public Guid AccountKeyId { get; set; } = Guid.Empty;
        [JsonIgnore]
        public List<EncryptionKey> AccountKeys { get; set; } = new List<EncryptionKey>();

        //Db interface interface
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Version { get; set; }
        public bool? Deleted { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }

        public IDictionary<string, IToken> Tokens { get; set; }
        [JsonIgnore]
        public ConcurrentDictionary<string, ITransaction> Transactions { get; set; }

        public abstract event EventHandler<IAccount> DetailsLoaded;
        public abstract event EventHandler<NewTransactionDTO> TxDetailsLoaded;
        public abstract event EventHandler<NewTransactionDTO> ConfirmedTransaction;

        public abstract bool IsLocked();
        public abstract Task<string> StartRefreshingData(int interval = 1000);
    }
}
