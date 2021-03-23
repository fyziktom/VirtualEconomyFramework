using log4net;
using Neblio.RestApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public class NeblioTransaction : CommonTransaction
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

            client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
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

        public override event EventHandler<NewTransactionDTO> DetailsLoaded;
        public override event EventHandler<NewTransactionDTO> ConfirmedTransaction;

        private static HttpClient httpClient = new HttpClient();
        private static IClient client;
        private static NeblioCryptocurrency NeblioCrypto = new NeblioCryptocurrency(false);

        private bool loading = false;

        public bool Loading
        {
            get
            {
               return loading;
            }
        }

        private async Task<bool> LoadRoutine()
        {
            var conf = Confirmations;
            loading = true;
            var dto = await LoadInfoFromAPI();

            if (Confirmations < 2)
            {
                return false;
            }
            else
            {
                loading = false;
                if (conf < 2)
                {
                    ConfirmedTransaction?.Invoke(this, dto);
                    return true;
                }
            }

            return false;
        }

        public override async Task GetInfo()
        {
            _ = Task.Run(async () =>
            {
                loading = true;
                var loaded = false;
                while (!loaded)
                {
                    var res = false;

                    try { 
                        res = await LoadRoutine();
                    }
                    catch (Exception ex)
                    {
                        // todo
                    }

                    if (res)
                    {
                        loaded = true;
                        break;
                    }
                    else
                    {
                        await Task.Delay(2000);
                    }
                }

                loading = false;
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

            var dto = new NewTransactionDTO();
            dto.AccountAddress = Address;
            dto.WalletName = WalletName;
            dto.Type = TransactionTypes.Neblio;
            dto.TxId = TxId;

            try
            {
                ITransaction txd = await NeblioTransactionHelpers.TransactionInfoAsync(client, TransactionTypes.Neblio, TxId);

                if (txd != null)
                {
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
                }
            }
            catch (Exception ex)
            {
                //log.Error("Cannot load tx details: ", ex);
            }

            if (dto.TransactionDetails != null)
            {
                DetailsLoaded?.Invoke(this, dto);
            }

            return dto;
        }

    }
}
