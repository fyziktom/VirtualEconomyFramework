using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.Coins;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Nodes;
using VEDrivers.Security;

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
        string WalletName { get; set; }
        Guid WalletId { get; set; }
        AccountTypes Type { get; set; }
        Guid OwnerId { get; set; }
        double NumberOfTransaction { get; set; }
        double NumberOfLoadedTransaction { get; }
        bool LoadingData { get; set; }
        string SpendableTxId { get; set; }
        string LastProcessedTxId { get; set; }
        string LastConfirmedTxId { get; set; }
        [JsonIgnore]
        EncryptionKey AccountKey { get; set; }
        [JsonIgnore]
        Guid AccountKeyId { get; set; }
        [JsonIgnore]
        List<EncryptionKey> AccountKeys { get; set; }
        IDictionary<string, IToken> Tokens { get; set; }

        [JsonIgnore]
        ConcurrentDictionary<string, ITransaction> Transactions { get; set; }

        event EventHandler<IAccount> DetailsLoaded;
        event EventHandler<NewTransactionDTO> TxDetailsLoaded;
        event EventHandler<NewTransactionDTO> ConfirmedTransaction;

        bool IsLocked();
        Task<string> StartRefreshingData(int interval = 1000);

    }
}
