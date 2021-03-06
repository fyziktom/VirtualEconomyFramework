﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Wallets
{
    public enum WalletTypes
    {
        Bitcoin,   
        Neblio,
        ReddCoin
    }

    public interface IWallet : IEconomyBase, IBalance, ICommonDbObjectBase
    {
        Guid Id { get; set; }
        Guid Owner { get; set; }
        WalletTypes Type { get; set; }
        string ConnectionAddress { get; }
        string ConnectionUrlBaseAddress { get; set; }
        int ConnectionPort { get; set; }
        int NumberOfActiveAccounts { get; set; }
        bool UseRPC { get; set; }
        ConcurrentDictionary<string, IAccount> Accounts { get; set; }

        event EventHandler<NewTransactionDTO> NewTransaction;
        event EventHandler<NewTransactionDTO> NewConfirmedTransactionDetailsReceived;

        void RegisterAccountEvents(string address);
        Task<IWallet> GetDetails();
        Task<ITransaction> GetTxDetails(string txid);
        Task<IDictionary<string, IAccount>> ListAccounts(bool useRPC = true, bool withTx = false);
    }
}
