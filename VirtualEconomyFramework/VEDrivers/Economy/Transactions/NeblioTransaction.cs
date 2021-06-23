using log4net;
using VEDriversLite.NeblioAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public class NeblioTransaction : CommonTransaction, IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public NeblioTransaction(string txid, string address, string walletName, bool justDto = true)
        {
            if (string.IsNullOrEmpty(txid))
                throw new Exception("New TX - txid cannod be empty or null");

            TimeStamp = DateTime.UtcNow;
            TxId = txid;
            //Currency = currency;
            Type = TransactionTypes.Neblio;
            Direction = TransactionDirection.Incoming;
            From = new List<string>();
            To = new List<string>();
            VinTokens = new List<IToken>();
            VoutTokens = new List<IToken>();
            Address = address;
            WalletName = walletName;
        }

        public NeblioTransaction(string txid, List<string> from, List<string> to, ConcurrentDictionary<string,string> metadata, double ammount)
        {
            if (string.IsNullOrEmpty(txid))
                throw new Exception("New TX - txid cannod be empty or null");

            TimeStamp = DateTime.UtcNow;
            From = from;
            To = to;
            TxId = txid;
            //Currency = currency;
            Metadata = metadata;
            Amount = ammount;
            Type = TransactionTypes.Neblio;
            VinTokens = new List<IToken>();
            VoutTokens = new List<IToken>();
        }

        private bool _disposed = false;
        ~NeblioTransaction() => Dispose(false);

        public override event EventHandler<NewTransactionDTO> DetailsLoaded;
        public override event EventHandler<NewTransactionDTO> ConfirmedTransaction;

        private int attempts = 5;

        private async Task<bool> LoadRoutine()
        {
            var conf = Confirmations;
            Loading = true;
            var dto = await LoadInfoFromAPI();

            if (dto != null)
            {
                if (Confirmations < EconomyMainContext.NumberOfConfirmationsToAccept)
                {
                    (dto.TransactionDetails as NeblioTransaction).Dispose();
                    return false;
                }
                else
                {
                    Loading = false;
                    if (conf < EconomyMainContext.NumberOfConfirmationsToAccept)
                    {
                        if (InvokeLoadFinish)
                            ConfirmedTransaction?.Invoke(this, dto);
                        
                        (dto.TransactionDetails as NeblioTransaction).Dispose();
                        
                        return true;
                    }
                    else
                    {
                        (dto.TransactionDetails as NeblioTransaction).Dispose();
                        return true;
                    }
                }

                (dto.TransactionDetails as NeblioTransaction).Dispose();
            }

            return false;
        }

        public override async Task GetInfo()
        {
            attempts = 10;

            await Task.Delay(1);

            _ = Task.Run(async () =>
            {
                Loading = true;
                while (!Loaded)
                {
                    var res = false;

                    try { 
                        res = await LoadRoutine();
                    }
                    catch (Exception ex)
                    {
                        // todo
                        attempts--;
                    }

                    if (res)
                    {
                        Loaded = true;
                        break;
                    }
                    else
                    {
                        await Task.Delay(200);
                    }

                    if (attempts <= 0)
                    {
                        CantLoad = true;
                        break;
                    }
                }

                //client = null;
                Loading = false;
            });        
        }


        public async Task<NewTransactionDTO> LoadInfoFromAPI()
        {
            if (string.IsNullOrEmpty(WalletName))
                return null;
            if (string.IsNullOrEmpty(Address))
                return null;
            if (string.IsNullOrEmpty(TxId))
                return null;

            try
            {
                var txd = await NeblioTransactionHelpers.TransactionInfoAsync(null, TransactionTypes.Neblio, TxId, Address);

                if (txd != null)
                {
                    var dto = new NewTransactionDTO();
                    dto.AccountAddress = Address;
                    dto.WalletName = WalletName;
                    dto.Type = TransactionTypes.Neblio;
                    dto.TxId = TxId;

                    dto.TransactionDetails = txd;
                    
                    VinTokens = dto.TransactionDetails.VinTokens;
                    VoutTokens = dto.TransactionDetails.VoutTokens;
                    Amount = dto.TransactionDetails.Amount;
                    Confirmations = dto.TransactionDetails.Confirmations;
                    Direction = dto.TransactionDetails.Direction;
                    From = dto.TransactionDetails.From;
                    To = dto.TransactionDetails.To;
                    TimeStamp = dto.TransactionDetails.TimeStamp;
                    Metadata = dto.TransactionDetails.Metadata;

                    if (dto.TransactionDetails != null)
                    {
                        DetailsLoaded?.Invoke(this, dto);
                    }

                    return dto;
                }
            }
            catch (Exception ex)
            {
                attempts--;
                //log.Error("Cannot load tx details: ", ex);
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                From = null;
                To = null;
                VinTokens = null;
                VoutTokens = null;
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }
}
