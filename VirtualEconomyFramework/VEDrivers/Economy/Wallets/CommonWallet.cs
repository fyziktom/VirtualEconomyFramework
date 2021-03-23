using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Wallets
{
    public abstract class CommonWallet : IWallet
    {
        public Guid Id { get; set; }
        public Guid Owner { get; set; }
        public WalletTypes Type { get; set; }
        public string ConnectionUrlBaseAddress { get; set; } = "127.0.0.1";
        public int ConnectionPort { get; set; } = 6326;

        private string _connectionAddress = "http://127.0.0.1:6326/";
        public string ConnectionAddress 
        {
            get
            {
                _connectionAddress = $"http://{ConnectionUrlBaseAddress}:{ConnectionPort}/";
                return _connectionAddress;
            }
        }
        public int NumberOfActiveAccounts { get; set; } = 0;
        public bool UseRPC { get; set; } = true;
        public ConcurrentDictionary<string, IAccount> Accounts { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string BaseURL { get; set; } = string.Empty;

        //IBalance
        public double? TotalBalance { get; set; } = 0.0;
        public double? TotalUnconfirmedBalance { get; set; } = 0.0;
        public double? TotalSpendableBalance { get; set; } = 0.0;

        //Db interface interface
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Version { get; set; }
        public bool? Deleted { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime CreatedOn { get; set; }

        public abstract event EventHandler<NewTransactionDTO> NewTransaction;
        public abstract event EventHandler<NewTransactionDTO> NewConfirmedTransactionDetailsReceived;

        public abstract void RegisterAccountEvents(string address);
        public abstract Task<IWallet> GetDetails();
        public abstract Task<ITransaction> GetTxDetails(string txid);
        public abstract Task<IDictionary<string, IAccount>> ListAccounts(bool useRPC = true, bool withTx = false);
    }
}
