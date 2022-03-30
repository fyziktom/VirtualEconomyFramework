using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.Cryptocurrencies;
using VEDriversLite.Dto;
using VEDriversLite.Events;
using VEDriversLite.Messaging;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.NFT.DevicesNFTs;
using VEDriversLite.Security;
using VEDriversLite.WooCommerce;

namespace VEDriversLite.Neblio
{
    /// <summary>
    /// Basic function class for Neblio Account 
    /// </summary>
    public abstract class NeblioAccountBase
    {
        private static object _lock { get; set; } = new object();

        /// <summary>
        /// Neblio Address hash
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Loaded Secret, NBitcoin Class which carry Public Key and Private Key
        /// </summary>
        [JsonIgnore]
        public BitcoinSecret Secret { get; set; }

        /// <summary>
        /// Number of the transactions on the address. not used now
        /// </summary>
        [JsonIgnore]
        public double NumberOfTransaction { get; set; } = 0;
        /// <summary>
        /// Number of already loaded transaction on the address. not used now
        /// </summary>
        [JsonIgnore]
        public double NumberOfLoadedTransaction { get; } = 0;
        /// <summary>
        /// If the address has enought Neblio to buy source VENFT tokens (costs 1 NEBL) this is set as true.
        /// </summary>
        [JsonIgnore]
        public bool EnoughBalanceToBuySourceTokens { get; set; } = false;
        /// <summary>
        /// Total actual balance based on Utxos. This means sum of spendable and unconfirmed balances.
        /// </summary>
        public double TotalBalance { get; set; } = 0.0;
        /// <summary>
        /// Total spendable balance based on Utxos.
        /// </summary>
        public double TotalSpendableBalance { get; set; } = 0.0;
        /// <summary>
        /// Total balance which is now unconfirmed based on Utxos.
        /// </summary>
        public double TotalUnconfirmedBalance { get; set; } = 0.0;
        /// <summary>
        /// Total balance of VENFT tokens which can be used for minting purposes.
        /// </summary>
        public double SourceTokensBalance { get; set; } = 0.0;
        /// <summary>
        /// Total balance of Coruzant tokens which can be used for minting purposes.
        /// </summary>
        [JsonIgnore]
        public double CoruzantSourceTokensBalance { get; set; } = 0.0;
        /// <summary>
        /// Total number of NFT on the address. It counts also Profile NFT, etc.
        /// </summary>
        [JsonIgnore]
        public double AddressNFTCount { get; set; } = 0.0;
        /// <summary>
        /// When main refreshing loop is running this is set
        /// </summary>
        [JsonIgnore]
        public bool IsRefreshingRunning { get; set; } = false;
        /// <summary>
        /// If you want to run account without NFTs set this up. 
        /// Whenever during run you can clear this flag and NFTs will start loading
        /// </summary>
        public bool WithoutNFTs { get; set; } = false;
        /// <summary>
        /// If there is some Doge address in same project which should be searched for the payments triggers fill it here
        /// </summary>
        public string ConnectedDogeAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// List of actual address NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// List of actual address Coruzant NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        public List<INFT> CoruzantNFTs { get; set; } = new List<INFT>();

        /// <summary>
        /// List of actual address HARDWARIO NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        public List<INFT> HardwarioNFTs { get; set; } = new List<INFT>();
        
        /// <summary>
        /// Received payments (means Payment NFT) of this address.
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// Received receipts (means Receipt NFT) of this address.
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedReceipts = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// If address has some profile NFT, it is founded in Utxo list and in this object.
        /// </summary>
        [JsonIgnore]
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");
        /// <summary>
        /// Actual all token supplies. Consider also other tokens than VENFT.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, TokenSupplyDto> TokensSupplies { get; set; } = new Dictionary<string, TokenSupplyDto>();
        /// <summary>
        /// Actual list of all Utxos on this address.
        /// </summary>
        [JsonIgnore]
        public List<Utxos> Utxos { get; set; } = new List<Utxos>();
        /// <summary>
        /// Service which gets prices of cryptocurrencies
        /// </summary>
        [JsonIgnore]
        public PriceService ExchangePriceService { get; set; } = new PriceService();
        /// <summary>
        /// Actual loaded address info. It has inside list of all transactions.
        /// </summary>
        [JsonIgnore]
        public GetAddressResponse AddressInfo { get; set; } = new GetAddressResponse();
        /// <summary>
        /// Actual loaded address info with list of Utxos. When utxos are loaded first, this is just fill with it to prevent not necessary API request.
        /// </summary>
        [JsonIgnore]
        public GetAddressInfoResponse AddressInfoUtxos { get; set; } = new GetAddressInfoResponse();
        /// <summary>
        /// This event is fired whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;
        /// <summary>
        /// This event is fired whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        public event EventHandler<string> NFTsChanged;

        /// <summary>
        /// This event is fired whenever some progress during multimint happens
        /// </summary>
        public event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is fired whenever price from exchanges is refreshed. It provides dictionary of the actual available rates.
        /// </summary>
        public event EventHandler<IDictionary<CurrencyTypes, double>> PricesRefreshed;

        /// <summary>
        /// This event is fired whenever profile nft is updated or found
        /// </summary>
        public event EventHandler<INFT> ProfileUpdated;
        /// <summary>
        /// This event is fired whenever some NFT is in received payment too and it should be blocked for any further action.
        /// It provides Utxo and UtxoIndex as touple.
        /// Event is fired also for the SubAccounts when it is registred from Main Account
        /// </summary>
        public event EventHandler<(string,int)> NFTAddedToPayments;

        /// <summary>
        /// This event is fired during first loading of the account to keep updated the user
        /// </summary>
        public event EventHandler<string> FirsLoadingStatus;

        /// <summary>
        /// Carrier for encrypted private key from storage and its password.
        /// </summary>
        [JsonIgnore]
        public EncryptionKey AccountKey { get; set; }

        /// <summary>
        /// This function will check if the account is locked or unlocked.
        /// </summary>
        /// <returns></returns>
        public bool IsLocked()
        {
            if (AccountKey != null)
            {
                if (AccountKey.IsEncrypted)
                {
                    if (AccountKey.IsPassLoaded)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (AccountKey.IsLoaded)
                        return false;
                    else
                        return true;
                }
            }
            else
                return true;
        }

        #region InfoEvents
        /// <summary>
        /// Redirect Info Event from lower layer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FireInfoEvent(object sender, IEventInfo e)
        {
            NewEventInfo?.Invoke(sender, e);
        }

        /// <summary>
        /// Invoke Success message info event
        /// </summary>
        /// <param name="txid">new tx id hash</param>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        public async Task InvokeSendPaymentSuccessEvent(string txid, string title = "Neblio Payment Sent")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Info,
                                                    title,
                                                    $"Successful send. Please wait a while for enough confirmations.",
                                                    Address,
                                                    txid,
                                                    100));
        }

        /// <summary>
        /// Invoke Error message because account is locked
        /// </summary>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        public async Task InvokeAccountLockedEvent(string title = "Cannot send transaction")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Error,
                                                    title,
                                                    "Account is Locked. Please unlock it in account page.",
                                                    Address,
                                                    string.Empty,
                                                    100));
        }
        /// <summary>
        /// Invoke Error message which occured during sending of the transaction
        /// </summary>
        /// <param name="errorMessage">Error message content</param>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        public async Task InvokeErrorDuringSendEvent(string errorMessage, string title = "Cannot send transaction")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Error,
                                                    title,
                                                    errorMessage,
                                                    Address,
                                                    string.Empty,
                                                    100));
        }
        /// <summary>
        /// Invoke Common Error message
        /// </summary>
        /// <param name="errorMessage">Error message content</param>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        public async Task InvokeErrorEvent(string errorMessage, string title = "Error")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Error,
                                                    title,
                                                    errorMessage,
                                                    Address,
                                                    string.Empty,
                                                    100));
        }


        #endregion

        #region LoadAccount

        /// <summary>
        /// This function will take all utxos and try to request all their txinfo and metadata info as parallel as possible
        /// The responses from the Neblio API are stored in the cashe, so most of them are not need to call anymore and they will be taken from the memory
        /// This speed up a loading a lot
        /// </summary>
        /// <returns></returns>
        public async Task TxCashPreload()
        {
            // cash preload just for the NFT utxos?
            //var nftutxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address, NFTHelpers.AllowedTokens, new GetAddressInfoResponse() { Utxos = Utxos });

            Console.WriteLine("Cash of the TxInfo preload started...");
            
            if (Utxos != null && Utxos.Count > 1)
            {
                var txinfotasks = new ConcurrentQueue<Task>();
                foreach(var utxo in Utxos)
                {
                    txinfotasks.Enqueue(NeblioTransactionHelpers.GetTransactionInfo(utxo.Txid));
                    var tokid = utxo.Tokens?.FirstOrDefault()?.TokenId;
                    if (!string.IsNullOrEmpty(tokid))
                    {
                        if (!VEDLDataContext.NFTCache.ContainsKey(utxo.Txid))
                            txinfotasks.Enqueue(NeblioTransactionHelpers.GetTokenMetadataOfUtxoCache(tokid, utxo.Txid));
                    }
                }

                var tasks = new ConcurrentQueue<Task>();
                var added = 0;
                var paralelism = 10;
                while (txinfotasks.Count > 0)
                {
                    if (txinfotasks.TryDequeue(out var tsk))
                    {
                        tasks.Enqueue(tsk);
                        added++;
                    }
                    if (added >= paralelism || txinfotasks.Count == 0)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                        added = 0;
                    }
                }

            }
            Console.WriteLine("Cash of the TxInfo preload end...");
        }

        /// <summary>
        /// Load account Key. If there is password it is used to decrypt the private key
        /// This function will load Secret property with Key
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <param name="key">Private Key</param>
        /// <returns></returns>
        public bool LoadAccountKey(string password, string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(password))
                    AccountKey = new EncryptionKey(key, fromDb: true);
                else
                    AccountKey = new EncryptionKey(key, fromDb: false);
                if (!string.IsNullOrEmpty(password))
                    AccountKey.LoadPassword(password);

                Secret = new BitcoinSecret(AccountKey.GetEncryptedKey(), NeblioTransactionHelpers.Network);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot load the Key for the account {Address}. " + ex.Message);
            }
        }

        #endregion

        #region AccountStatsLoad

        /// <summary>
        /// Register event of the PriceService PriceRefreshed. Then the event is resend by NeblioAccountBase class
        /// </summary>
        public void RegisterPriceServiceEventHandler()
        {
            if (ExchangePriceService != null)
            {
                ExchangePriceService.PricesRefreshed -= ExchangePriceService_PricesRefreshed;
                ExchangePriceService.PricesRefreshed += ExchangePriceService_PricesRefreshed;
            }
            else
                Console.WriteLine("Cannot register Event Handler for PriceRefreshed because the ExchangePriceService is null.");
        }

        private void ExchangePriceService_PricesRefreshed(object sender, IDictionary<CurrencyTypes, double> e)
        {
            PricesRefreshed?.Invoke(sender, e);
        }

        /// <summary>
        /// Reload actual token supplies based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadTokenSupply()
        {
            var tos = await NeblioTransactionHelpers.CheckTokensSupplies(Address, AddressInfoUtxos);
            lock (_lock)
            {
                TokensSupplies = tos;
            }
            
            if (TokensSupplies.TryGetValue(CoruzantNFTHelpers.CoruzantTokenId, out var ts))
                CoruzantSourceTokensBalance = ts.Amount;
        }

        /// <summary>
        /// Reload actual count of the NFTs based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadCountOfNFTs()
        {
            var nftsu = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address, NFTHelpers.AllowedTokens, AddressInfoUtxos);
            if (nftsu != null)
                AddressNFTCount = nftsu.Count;
        }

        /// <summary>
        /// Reload actual VENFT minting supply based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadMintingSupply()
        {
            var mintingSupply = await NeblioTransactionHelpers.GetActualMintingSupply(Address, NFTHelpers.TokenId, AddressInfoUtxos);
            SourceTokensBalance = mintingSupply.Item1;

        }

        /// <summary>
        /// Reload coruzant NFTs list
        /// </summary>
        /// <returns></returns>
        public async Task ReloadCoruzantNFTs()
        {
            var nftc = await CoruzantNFTHelpers.GetCoruzantNFTs(NFTs);
            if (nftc != null)
            {
                lock (_lock)
                {
                    CoruzantNFTs = nftc;
                }
            }
        }

        /// <summary>
        /// Reload hardwario NFTs list
        /// </summary>
        /// <returns></returns>
        public async Task ReloadHardwarioNFTs()
        {
            var nftc = await HardwarioNFTHelpers.GetHARDWARIONFTs(NFTs);
            if (nftc != null)
            {
                lock (_lock)
                {
                    HardwarioNFTs = nftc;
                }
            }
        }

        /// <summary>
        /// Reload address Utxos list. It will sort descending the utxos based on the utxos time stamps.
        /// </summary>
        /// <returns></returns>
        public async Task ReloadUtxos()
        {
            var ux = await NeblioTransactionHelpers.GetAddressUtxosObjects(Address);
            var ouxox = ux.OrderBy(u => u.Blocktime).Reverse().ToList();

            if (ouxox.Count > 0)
            {
                lock (_lock)
                {
                    Utxos.Clear();
                    TotalBalance = 0.0;
                    TotalUnconfirmedBalance = 0.0;
                    TotalSpendableBalance = 0.0;
                    // add new ones
                    foreach (var u in ouxox)
                    {
                        Utxos.Add(u);
                        if (u.Blockheight <= 0)
                            TotalUnconfirmedBalance += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);
                        else
                            TotalSpendableBalance += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);
                    }

                    TotalBalance = TotalSpendableBalance + TotalUnconfirmedBalance;

                    AddressInfoUtxos = new GetAddressInfoResponse()
                    {
                        Utxos = Utxos
                    };
                }
            }
        }


        /// <summary>
        /// This function will load actual address info an adress utxos. It is used mainly for loading list of all transactions.
        /// </summary>
        /// <returns></returns>
        public async Task ReloadAccountInfo()
        {
            var ai = await NeblioTransactionHelpers.AddressInfoAsync(Address);
            
            if (ai != null)
            {
                lock (_lock)
                {
                    AddressInfo = ai;
                }

                if (AddressInfo != null)
                {
                    lock (_lock)
                    {
                        TotalBalance = (double)AddressInfo.Balance;
                        TotalUnconfirmedBalance = (double)AddressInfo.UnconfirmedBalance;
                        AddressInfo.Transactions = AddressInfo.Transactions.Reverse().ToList();
                    }
                }
                else
                    AddressInfo = new GetAddressResponse();

                if (TotalBalance > 1)
                    EnoughBalanceToBuySourceTokens = true;
            }
        }

        /// <summary>
        /// This function will reload changes in the NFTs list based on provided list of already loaded utxos.
        /// </summary>
        /// <returns></returns>
        public async Task ReLoadNFTs(bool fireProfileEvent = false, bool withoutMessages = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(Address))
                {
                    if (fireProfileEvent)
                    {
                        NFTHelpers.ProfileNFTFound -= NFTHelpers_ProfileNFTFound;
                        NFTHelpers.ProfileNFTFound += NFTHelpers_ProfileNFTFound;
                    }

                    NFTHelpers.NFTLoadingStateChanged -= NFTHelpers_LoadingStateChangedHandler;
                    NFTHelpers.NFTLoadingStateChanged += NFTHelpers_LoadingStateChangedHandler;

                    var lastnft = NFTs.FirstOrDefault();
                    var lastcount = NFTs.Count;
                    var nfts = await NFTHelpers.LoadAddressNFTs(Address, Utxos, NFTs.ToList(), fireProfileEvent, withoutMessages:withoutMessages);
                    if (nfts == null)
                        return;
                    lock (_lock)
                    {
                        NFTs = nfts;
                    }
                    if (lastnft != null)
                    {
                        var nft = NFTs.FirstOrDefault();
                        //Console.WriteLine("Last time: " + lastnft.Time.ToString());
                        //Console.WriteLine("Newest time: " + nft.Time.ToString());
                        if (nft != null)
                        {
                            if (nft.Time != lastnft.Time)
                                NFTsChanged?.Invoke(this, "Changed");
                            else
                            {
                                if (NFTs.Count != lastcount)
                                    NFTsChanged?.Invoke(this, "Changed");
                            }
                        }
                        else
                        {
                            if (NFTs.Count != lastcount)
                                NFTsChanged?.Invoke(this, "Changed");
                        }
                    }
                    else if (lastnft == null && NFTs.Count > 0)
                    {
                        NFTsChanged?.Invoke(this, "Changed");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reload NFTs. " + ex.Message);
            }
            finally
            {
                if (fireProfileEvent)
                    NFTHelpers.ProfileNFTFound -= NFTHelpers_ProfileNFTFound;
                NFTHelpers.NFTLoadingStateChanged -= NFTHelpers_LoadingStateChangedHandler;
            }
        }

        private void NFTHelpers_ProfileNFTFound(object sender, INFT e)
        {
            var add = sender as string;
            if (!string.IsNullOrEmpty(add) && add == Address)
            {
                Profile = e as ProfileNFT;
                ProfileUpdated?.Invoke(this, e);
            }
        }
        private void NFTHelpers_LoadingStateChangedHandler(object sender, string e)
        {
            var add = sender as string;
            if (!string.IsNullOrEmpty(add) && add == Address)
            {
                FirsLoadingStatus?.Invoke(this, e);
            }
        }

        /// <summary>
        /// This function will search NFT Payments in the NFTs list and load them into ReceivedPayments list. 
        /// This list is cleared at the start of this function
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAddressReceivedPayments()
        {
            try
            {
                lock (_lock)
                {
                    var firstpnft = ReceivedPayments.Values.FirstOrDefault();
                    var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
                    var ffirstpnft = pnfts.FirstOrDefault();

                    //if ((firstpnft != null && ffirstpnft != null) || (firstpnft == null && ffirstpnft != null))
                    {
                        //if ((firstpnft == null && ffirstpnft != null) || (firstpnft != null && (firstpnft.Utxo != ffirstpnft.Utxo)))
                        {
                            ReceivedPayments.Clear();
                            foreach (var p in pnfts)
                            {
                                ReceivedPayments.TryAdd(p.NFTOriginTxId, p);
                                var _nft = NFTs.Where(nft => NFTHelpers.IsBuyableNFT(nft.Type))
                                               .FirstOrDefault(n => n.Utxo == (p as PaymentNFT).NFTUtxoTxId &&
                                                                    n.UtxoIndex == (p as PaymentNFT).NFTUtxoIndex);
                                if (_nft != null)
                                    NFTAddedToPayments?.Invoke(Address, ((p as PaymentNFT).NFTUtxoTxId, (p as PaymentNFT).NFTUtxoIndex));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot refresh address received payments. " + ex.Message);
            }
        }

        /// <summary>
        /// Redirect of the Event about added NFT to the payments
        /// It can inform asap UI to block buy of the NFT if it is just original
        /// </summary>
        /// <param name="address">Address of the SubAccount</param>
        /// <param name="e">Utxo hash and Utxo Index</param>
        public void FireNFTAddedToPayments(string address, (string,int) e)
        {
            NFTAddedToPayments?.Invoke(address, e);
        }

        /// <summary>
        /// This function will search NFT Receipts in the NFTs list and load them into ReceivedReceipts list. 
        /// This list is cleared at the start of this function
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAddressReceivedReceipts()
        {
            try
            {
                lock (_lock)
                {
                    var firstpnft = ReceivedPayments.Values.FirstOrDefault();
                    var pnfts = NFTs.Where(n => n.Type == NFTTypes.Receipt).ToList();
                    var ffirstpnft = pnfts.FirstOrDefault();

                    if ((firstpnft != null && ffirstpnft != null) || firstpnft == null && ffirstpnft != null)
                    {
                        if ((firstpnft == null && ffirstpnft != null) || (firstpnft != null && (firstpnft.Utxo != ffirstpnft.Utxo)))
                        {
                            ReceivedReceipts.Clear();
                            foreach (var p in pnfts)
                            {
                                if (!string.IsNullOrEmpty(p.NFTOriginTxId))
                                    ReceivedReceipts.TryAdd(p.Utxo, p);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot refresh address received receipts. " + ex.Message);
            }
        }

        #endregion

        #region TransactionSourcesCheck


        /// <summary>
        /// This function will check if the address has some spendable neblio for transaction.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(bool, double)> HasSomeSpendableNeblio(double amount = 0.0002)
        {
            var nutxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(Address, 0.0001, amount);
            if (nutxos.Count == 0)
            {
                return (false, 0.0);
            }
            else
            {
                var a = 0.0;
                foreach (var u in nutxos)
                    a += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);

                if (a > amount)
                    return (true, a);
                else
                    return (false, a);
            }
        }

        /// <summary>
        /// This function will check if the address has some spendable VENFT tokens for minting. 
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, int)> HasSomeSourceForMinting()
        {
            var tutxos = await NeblioTransactionHelpers.FindUtxoForMintNFT(Address, NFTHelpers.TokenId, 1);

            if (tutxos.Count == 0)
                return (false, 0);
            else
            {
                var a = 0;
                foreach (var u in tutxos)
                {
                    var t = u.Tokens.ToArray()[0];
                    a += (int)t.Amount;
                }
                return (true, a);
            }
        }

        /// <summary>
        /// This function will validate if the NFT of this address is spendable
        /// </summary>
        /// <param name="utxo">Utxo hash</param>
        /// <param name="index">index of the Utxo</param>
        /// <returns></returns>
        public async Task<(bool, string)> ValidateNFTUtxo(string utxo, int index)
        {
            var u = await NeblioTransactionHelpers.ValidateOneTokenNFTUtxo(Address, NFTHelpers.TokenId, utxo, index, addinfo: AddressInfoUtxos);
            if (u >= 0)
            {
                var msg = $"Provided source tx transaction is not spendable. Probably waiting for more than {NeblioTransactionHelpers.MinimumConfirmations} confirmation.";
                return (false, msg);
            }
            else
                return (true, "OK");
        }

        /// <summary>
        /// This function will check if there is some spendable neblio of specific amount and returns list of the utxos for the transaction
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(string, ICollection<Utxos>)> CheckSpendableNeblio(double amount)
        {
            try
            {
                var nutxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(Address, 0.0002, amount, addinfo: AddressInfoUtxos);
                if (nutxos == null || nutxos.Count == 0)
                    return ($"You dont have Neblio on the address. Probably waiting for more than {NeblioTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                    return ("OK", nutxos);
            }
            catch (Exception ex)
            {
                return ("Cannot check spendable Neblio. " + ex.Message, null);
            }
        }

        /// <summary>
        /// This function will check if there is some spendable tokens of specific Id and amount and returns list of the utxos for the transaction.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(string, ICollection<Utxos>)> CheckSpendableNeblioTokens(string id, int amount)
        {
            try
            {
                var tutxos = await NeblioTransactionHelpers.FindUtxoForMintNFT(Address, id, amount, addinfo: AddressInfoUtxos);
                if (tutxos == null || tutxos.Count == 0)
                    return ($"You dont have Tokens on the address. You need at least 5 for minting. Probably waiting for more than {NeblioTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                    return ("OK", tutxos);
            }
            catch (Exception ex)
            {
                return ("Cannot check spendable Neblio Tokens. " + ex.Message, null);
            }
        }

        #endregion

        #region EncryptionAndVerification

        /// <summary>
        /// Verify message which was signed by some address.
        /// </summary>
        /// <param name="message">Input message</param>
        /// <param name="signature">Signature of this message created by owner of some address.</param>
        /// <param name="address">Neblio address which should sign the message and should be verified.</param>
        /// <param name="bobPubKey">You must fill address or PubKey. If you will fill the PubKey function is much faster</param>
        /// <returns></returns>
        public async Task<(bool, string)> VerifyMessage(string message, string signature, string address, PubKey bobPubKey = null)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || (string.IsNullOrEmpty(address) && bobPubKey == null))
                return (false, "You must fill all inputs. You can fill just one of these Address or PubKey.");
            if (bobPubKey == null)
            {
                var ownerpubkey = await NFTHelpers.GetPubKeyFromProfileNFTTx(address);
                if (!ownerpubkey.Item1)
                    return (false, "Owner did not activate the function. He must have filled the profile.");
                bobPubKey = ownerpubkey.Item2;
            }
            return await ECDSAProvider.VerifyMessage(message, signature, bobPubKey);
        }

        /// <summary>
        /// Encrypt message with use of ECDSA
        /// </summary>
        /// <param name="message">Input message</param>
        /// <returns></returns>
        public async Task<(bool, string)> EncryptMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return (false, "You must fill the message.");
            if (Secret == null)
                return (false, "Account is not loaded.");
            return await ECDSAProvider.EncryptMessage(message, Secret.PubKey.ToString());
        }

        /// <summary>
        /// Decrypt message with use of ECDSA
        /// </summary>
        /// <param name="emessage">Input message</param>
        /// <returns></returns>
        public async Task<(bool, string)> DecryptMessage(string emessage)
        {
            if (string.IsNullOrEmpty(emessage))
                return (false, "You must fill the encrypted message.");
            if (Secret == null)
                return (false, "Account is not loaded.");
            return await ECDSAProvider.DecryptMessage(emessage, Secret);
        }

        /// <summary>
        /// Obtain verify code of some transaction. This will combine txid and UTC time (rounded to minutes) and sign with the private key.
        /// It will create unique code, which can be verified and it is valid just one minute.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<OwnershipVerificationCodeDto> GetNFTVerifyCode(string txid)
        {
            if (string.IsNullOrEmpty(txid))
                return new OwnershipVerificationCodeDto();
            if (Secret == null)
                return new OwnershipVerificationCodeDto();
            var res = await OwnershipVerifier.GetCodeInDto(txid, Secret);
            if (res != null)
                return res;
            else
                return new OwnershipVerificationCodeDto();
        }

        /// <summary>
        /// Verification function for the NFT ownership code generated by GetNFTVerifyCode function.
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public async Task<(OwnershipVerificationCodeDto, byte[])> GetNFTVerifyQRCode(string txid)
        {
            if (string.IsNullOrEmpty(txid))
                return (new OwnershipVerificationCodeDto(), new byte[0]);
            if (Secret == null)
                return (new OwnershipVerificationCodeDto(), new byte[0]);
            var res = await OwnershipVerifier.GetQRCode(txid, Secret);
            if (res.Item1)
                return (res.Item2.Item1, res.Item2.Item2);
            else
                return (new OwnershipVerificationCodeDto(), new byte[0]);
        }

        /// <summary>
        /// Sign custom message with use of account Private Key
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SignMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return (false, "You must fill the message.");
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var key = AccountKey.GetEncryptedKey();
            return await ECDSAProvider.SignMessage(message, key);
        }

        #endregion

        #region Transactions

        /// <summary>
        /// Send classic neblio payment
        /// </summary>
        /// <param name="receiver">Receiver Neblio Address</param>
        /// <param name="amount">Ammount in Neblio</param>
        /// <param name="message">Message in transaction data</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNeblioPayment(string receiver, double amount, string message = "")
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(amount + 0.002);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }

            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = amount,
                SenderAddress = Address,
                ReceiverAddress = receiver,
                CustomMessage = message
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNeblioTransactionAPIAsync(dto, AccountKey, res.Item2);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Payment Sent");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Send multiuple input neblio payment
        /// </summary>
        /// <param name="receiver">Receiver Neblio Address</param>
        /// <param name="amount">Ammount in Neblio</param>
        /// <param name="utxos">Input utxos</param>
        /// <param name="message">Message in transaction data</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendMultipleInputNeblioPayment(string receiver, double amount, List<Utxos> utxos, string message = "")
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(amount + 0.002);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }

            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = amount,
                SenderAddress = Address,
                ReceiverAddress = receiver,
                CustomMessage = message
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNeblioTransactionAPIAsync(dto, AccountKey, utxos, 20000);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Payment Sent");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Split neblio coin to smaller coins
        /// </summary>
        /// <param name="receivers">Receivers list of Neblio Address</param>
        /// <param name="lots">Number of the lots</param>
        /// <param name="amount">Amount of new splited couns</param>
        /// <returns></returns>
        public async Task<(bool, string)> SplitNeblioCoin(List<string> receivers, int lots, double amount)
        {
            if (amount < 0.0005)
                return (false, "Minimal output splitted coin amount is 0.0005 NEBL.");
            if (lots < 2 || lots > NeblioTransactionHelpers.MaximumNeblioOutpus)
                return (false, "Minimal count of output splitted coin amount is 2. Maximum is 25.");

            var totalAmount = amount * lots;

            if (totalAmount >= TotalSpendableBalance)
                return (false, $"Cannot send transaction. Total Amount is bigger than spendable neblio on this address. Total Amount {totalAmount}.");

            if ((receivers.Count > 1 && lots > 1) && (receivers.Count != lots))
                return (false, $"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receivers.Count}, Lost {lots}.");

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }

            var res = await CheckSpendableNeblio(totalAmount + 0.002);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable inputs");
                return (false, res.Item1);
            }

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SplitNeblioCoinTransactionAPIAsync(Address, receivers, lots, amount, AccountKey, res.Item2, 20000);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Split Sent");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Send classic token payment. It must match same requirements as minting. It cannot use 1 token inputs (NFTs).
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <param name="metadata">Custom metadata</param>
        /// <param name="receiver">Receiver Neblio address</param>
        /// <param name="amount">Amount of the tokens</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNeblioTokenPayment(string tokenId, IDictionary<string, string> metadata, string receiver, int amount)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, amount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable token inputs");
                return (false, tres.Item1);
            }

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = Convert.ToDouble(amount),
                SenderAddress = Address,
                ReceiverAddress = receiver,
                Metadata = metadata,
                Id = tokenId
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendTokenLotAsync(dto, AccountKey, res.Item2, tres.Item2);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Token Payment Sent");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Send split token payment. It will create multiple outputs with lots of tokens.
        /// It must match same requirements as minting. It cannot use 1 token inputs (NFTs).
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <param name="metadata">Custom metadata</param>
        /// <param name="receivers">List Receiver Neblio address</param>
        /// <param name="lots">Amount of the tokens</param>
        /// <param name="amount">Amount of the tokens</param>
        /// <returns></returns>
        public async Task<(bool, string)> SplitTokens(string tokenId, IDictionary<string, string> metadata, List<string> receivers, int lots, int amount)
        {
            if (lots > NeblioTransactionHelpers.MaximumTokensOutpus)
                return (false, $"Cannot create more than {NeblioTransactionHelpers.MaximumTokensOutpus} lots.");

            var totalAmount = amount * lots;
            if (!TokensSupplies.TryGetValue(tokenId, out var tsdto))
                return (false, $"Cannot send transaction. You do not have this kind of tokens. Token Id {tokenId}.");

            if (totalAmount >= tsdto.Amount)
                return (false, $"Cannot send transaction. Total Amount is bigger than available source. Total Amount {totalAmount}, Token Id {tokenId}.");

            if ((receivers.Count > 1 && lots > 1) && (receivers.Count != lots))
                return (false, $"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receivers.Count}, Lost {lots}.");

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, totalAmount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable token inputs");
                return (false, tres.Item1);
            }

            metadata.Add("VENFT App", "https://about.ve-nft.com/");
            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SplitNTP1TokensAsync(receivers, lots, amount, tokenId, metadata, AccountKey, res.Item2, tres.Item2);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio Split Token Payment Sent");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }


        #endregion

        #region NFTTransactions


        /// <summary>
        /// Mint new NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input
        /// </summary>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        public async Task<(bool, string)> MintNFT(INFT NFT, string receiver = "")
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(NFT.TokenId, 3);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs. You need 3 tokens as minimum input.");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.MintNFT(Address, AccountKey, nft, res.Item2, tres.Item2, receiver);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio NFT Minted");
                    if (NFT.Type == NFTTypes.Profile)
                        Profile = NFT as ProfileNFT;

                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Mint new multi NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input.
        /// </summary>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <param name="coppies">Number of coppies. 1 coppy means 2 final NFTs</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        public async Task<(bool, string)> MintMultiNFT(INFT NFT, int coppies, string receiver = "")
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(NFT.TokenId, 2 + coppies);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.MintMultiNFT(Address, coppies, AccountKey, nft, res.Item2, tres.Item2, receiver);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Neblio NFT Sent");
                    if (NFT.Type == NFTTypes.Profile)
                        Profile = NFT as ProfileNFT;

                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }


        private async Task<(bool, (ICollection<Utxos>, ICollection<Utxos>))> MultimintSourceCheck(string tokenId, int coppies)
        {
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null || res.Item2.Count == 0)
            {
                //await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, (null, null));
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, 3 + coppies);
            if (tres.Item2 == null || tres.Item2.Count == 0)
            {
                //await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs. You need 3 tokens as minimum input.");
                return (false, (null, null));
            }
            return (true, (res.Item2, tres.Item2));

        }

        /// <summary>
        /// Mint new multi NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input.
        /// </summary>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <param name="coppies">Number of coppies. 1 coppy means 2 final NFTs</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        public async Task<(bool, string)> MintMultiNFTLargeAmount(INFT NFT, int coppies, string receiver = "")
        {
            var nft = await NFTFactory.CloneNFT(NFT);
            try
            {
                if (IsLocked())
                {
                    await InvokeAccountLockedEvent();
                    return (false, "Account is locked.");
                }

                int cps = coppies;

                Console.WriteLine("Start of minting.");
                int lots = 0;
                int rest = 0;
                rest += cps % NeblioTransactionHelpers.MaximumTokensOutpus;
                lots += (int)((cps - rest) / NeblioTransactionHelpers.MaximumTokensOutpus);
                string txres = string.Empty;
                NewMintingProcessInfo?.Invoke(this, $"Minting of {lots} lots started...");

                var txsidsres = string.Empty;
                if (lots > 1 || (lots == 1 && rest > 0))
                {
                    var done = false;
                    for (int i = 0; i < lots; i++)
                    {
                        Console.WriteLine("-----------------------------");
                        Console.WriteLine($"Minting lot {i} from {lots}:");
                        done = false;
                        await Task.Run(async () =>
                        {
                            while (!done)
                            {
                                var sres = await MultimintSourceCheck(NFT.TokenId, NeblioTransactionHelpers.MaximumTokensOutpus);
                                if (sres.Item1)
                                {
                                    try
                                    {
                                        txres = await NFTHelpers.MintMultiNFT(Address, NeblioTransactionHelpers.MaximumTokensOutpus - 1, AccountKey, nft, sres.Item2.Item1, sres.Item2.Item2, receiver);
                                        if (string.IsNullOrEmpty(txres))
                                        {
                                            Console.WriteLine("Waiting for spendable utxo...");
                                            await Task.Delay(5000);
                                        }
                                        else
                                        {
                                            done = true;
                                            txsidsres += txres + "-";
                                            NewMintingProcessInfo?.Invoke(this, $"New Lot Minted: {txres}, Waiting for processing next {i + 1} of {lots} lots.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Cannot send Mint. Probably need to wait for the confirmation. Error: " + ex.Message);
                                        await Task.Delay(5000);
                                        done = false;
                                    }
                                }
                                else
                                {
                                    await Task.Delay(5000);
                                    done = false;
                                }
                            }
                        });
                    }
                    if (rest > 0)
                    {
                        Console.WriteLine($"Minting rest {rest} tickets.");
                        done = false;
                        await Task.Run(async () =>
                        {
                            while (!done)
                            {
                                var sres = await MultimintSourceCheck(NFT.TokenId, rest);
                                if (sres.Item1)
                                {
                                    txres = await NFTHelpers.MintMultiNFT(Address, rest, AccountKey, nft, sres.Item2.Item1, sres.Item2.Item2, receiver);
                                    if (string.IsNullOrEmpty(txres))
                                    {
                                        Console.WriteLine("Waiting for spendable utxo...");
                                        await Task.Delay(5000);
                                    }
                                    else
                                    {
                                        done = true;
                                        txsidsres += txres + "-";
                                        NewMintingProcessInfo?.Invoke(this, $"Rest of {rest} NFTs of total {coppies} NFTs was Minted: {txres}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Waiting for spendable utxo...");
                                    await Task.Delay(5000);
                                }
                            }
                        });
                    }
                }
                else
                {
                    var sres = await MultimintSourceCheck(NFT.TokenId, NeblioTransactionHelpers.MaximumTokensOutpus);
                    if (sres.Item1)
                        txres = await NFTHelpers.MintMultiNFT(Address, cps, AccountKey, nft, sres.Item2.Item1, sres.Item2.Item2, receiver);
                    else
                    {
                        await InvokeErrorDuringSendEvent("Cannot Mint NFTs", "Not enough spendable source.");
                        return (false, "Not enough spendable source.");
                    }
                    if (!string.IsNullOrEmpty(txres)) txsidsres += txres;
                    txsidsres = txsidsres.Trim('-');
                }

                if (txres != null)
                {
                    await InvokeSendPaymentSuccessEvent(txsidsres, "Neblio NFT Sent");
                    return (true, txsidsres);
                }
                else
                {
                    NewMintingProcessInfo?.Invoke(this, $"All NFTs minted...");
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }


        /// <summary>
        /// This function will destroy provided NFTs. It means it will connect them again to one output/lot of tokens.
        /// Now it is possible to destroy just same kind of tokens. The first provided NFT will define TokenId. Different tokensIds will be skipped.
        /// Maximum to destroy in one transaction is 10 of NFTs
        /// </summary>
        /// <param name="nfts">List of NFTs</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        public async Task<(bool, string)> DestroyNFTs(ICollection<INFT> nfts, string receiver = "")
        {
            if (nfts == null || nfts.Count == 0)
            {
                await InvokeErrorDuringSendEvent("Cannot Destroy NFTs", "No NFTs provided.");
                return (false, "No NFTs provided.");
            }
            var nft = nfts.First();
            if (nfts.Count != nfts.Where(n => nft.TokenId == n.TokenId).Count())
            {
                await InvokeErrorDuringSendEvent("Cannot Destroy NFTs", "Different NFTs provided. You can destroy just same kind of NFTs in one request.");
                return (false, "Different NFTs provided.");
            }
            
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(nft.TokenId, 3);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs for minting. You need one input from minting supply. That one will be merged with destroyed free tokens.");
                return (false, tres.Item1);
            }

            try
            {
                // send tx
                var rtxid = await NFTHelpers.DestroyNFTs(Address, AccountKey, nfts, res.Item2, receiver, tres.Item2.FirstOrDefault());
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFTs Destroyed.");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Cannot Destroy NFT");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }


        /// <summary>
        /// Send input NFT to new owner, or use it just for price write and resend it to yourself with new metadata about the price.
        /// </summary>
        /// <param name="receiver">If the pricewrite is set, this is filled with own address</param>
        /// <param name="NFT"></param>
        /// <param name="priceWrite">Set this if you need to just write the price to the NFT</param>
        /// <param name="price">Price must be bigger than 0.0002 NEBL</param>
        /// <param name="withDogePrice">Set this if you need to just write the Doge price to the NFT</param>
        /// <param name="dogeprice">Price must be bigger than 0.1 Doge</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNFT(string receiver, INFT NFT, bool priceWrite = false, double price = 0.0002, bool withDogePrice = false, double dogeprice = 1)
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot snd NFT without provided Utxo TxId.", "Cannot send NFT");
                return (false, "Cannot send NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            if (string.IsNullOrEmpty(receiver) || priceWrite)
                receiver = Address;

            try
            {
                var rtxid = await NFTHelpers.SendNFT(Address, receiver, AccountKey, nft, priceWrite, res.Item2, price, withDogePrice, dogeprice);

                if (rtxid != null)
                {
                    if (!priceWrite)
                        await InvokeSendPaymentSuccessEvent(rtxid, "NFT Sent");
                    else
                        await InvokeSendPaymentSuccessEvent(rtxid, "Price written to NFT");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }


        /// <summary>
        /// Change Post NFT. It requeires previous loadedPost NFT as input.
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task<(bool, string)> ChangeNFT(INFT NFT)
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot change Post NFT without provided Utxo TxId.", "Cannot change the Post NFT");
                return (false, "Cannot change NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.ChangeNFT(Address, AccountKey, nft, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Changed");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Send NFT.
        /// </summary>
        /// <param name="NFT">NFT to sent</param>
        /// <param name="sender">Sender of the NFT</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendIoTMessageNFT(INFT NFT, string sender, string receiver = "")
        {
            if (string.IsNullOrEmpty(receiver))
                receiver = sender;

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            var tres = await CheckSpendableNeblioTokens(NFT.TokenId, 3);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.SendIoTMessageNFT(Address, receiver, AccountKey, NFT, res.Item2, tres.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Message sent.");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during sending message.");
        }

        /// <summary>
        /// Send NFT Message.
        /// </summary>
        /// <param name="name">Name of the Message</param>
        /// <param name="message">Content of the Message</param>
        /// <param name="receiver">Receiver of the Message</param>
        /// <param name="utxo">original NFT Utxo - reply for the existing message</param>
        /// <param name="text">Longer text in the message</param>
        /// <param name="imagelink">Image link in the message</param>
        /// <param name="link">Link in the message</param>
        /// <param name="rewriteAuthor">Rewrite author of the message. Common is the sender (you can use txid of some NFT Profil, etc.)</param>
        /// <param name="encrypt">Encrypt the message with the shared secret</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendMessageNFT(string name, string message, string receiver, string utxo = "", bool encrypt = true, string imagelink = "", string link = "", string text = "", string rewriteAuthor = "")
        {
            MessageNFT nft = new MessageNFT("");

            nft.Utxo = utxo;
            nft.Description = message;
            nft.Name = name;
            nft.Text = text;
            nft.Link = link;
            nft.ImageLink = imagelink;
            nft.Encrypt = encrypt;
            nft.TokenId = NFTHelpers.TokenId;

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            var tres = await CheckSpendableNeblioTokens(NFTHelpers.TokenId, 3);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable Token inputs");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.SendMessageNFT(Address, receiver, AccountKey, nft, res.Item2, tres.Item2, rewriteAuthor:rewriteAuthor);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Message sent.");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during sending message.");
        }


        /// <summary>
        /// Write Used flag into NFT Ticket
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task<(bool, string)> UseNFTTicket(INFT NFT)
        {
            if (NFT.Type != NFTTypes.Ticket)
                throw new Exception("This is not NFT ticket.");

            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot snd NFT without provided Utxo TxId.", "Cannot send NFT");
                return (false, "Cannot send NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.UseNFTTicket(Address, AccountKey, nft, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Ticket used.");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// This function will send payment for some specific NFT which is from foreign address.
        /// </summary>
        /// <param name="receiver">Receiver - owner of the NFT</param>
        /// <param name="NFT">NFT what you want to buy</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNFTPayment(string receiver, INFT NFT)
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot send NFT Payment without provided Utxo TxId of this NFT", "Cannot send Payment for NFT");
                return (false, "Cannot send NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(NFT.Price + 0.002);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.SendNFTPayment(Address, AccountKey, receiver, nft, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Payment for NFT Sent");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// This function will return the NFT Payment to the original sender.
        /// The receiver is taken from the Minting transaction of the PaymentNFT
        /// </summary>
        /// <param name="NFT">NFT Payment to return</param>
        /// <param name="receiver">Receiver of the returned NFT Payment</param>
        /// <returns></returns>
        public async Task<(bool, string)> ReturnNFTPayment(string receiver, PaymentNFT NFT)
        {
            //var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }

            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                NFT.Returned = true;
                var rtxid = await NFTHelpers.ReturnNFTPayment(Address, AccountKey, NFT, res.Item2);

                if (rtxid != null)
                {
                    await ReLoadNFTs();
                    await RefreshAddressReceivedPayments();
                    await InvokeSendPaymentSuccessEvent(rtxid, "Payment returned to the original sender");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// This function will send airdrop.
        /// </summary>
        /// <param name="receiver">Receiver - owner of the NFT</param>
        /// <param name="tokenId">TokenId of the NTP1 token on Neblio network</param>
        /// <param name="tokenAmount">Number of the tokens in the airdrop</param>
        /// <param name="neblioAmount">Neblio amount in the airdrop</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendAirdrop(string receiver, string tokenId, double tokenAmount, double neblioAmount)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }

            var res = await CheckSpendableNeblio(neblioAmount + 0.002);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, (int)tokenAmount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enough spendable token inputs");
                return (false, tres.Item1);
            }

            var metadata = new Dictionary<string, string>();
            metadata.Add("Message", "Thank you for using VENFT. https://about.ve-nft.com/");

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = tokenId, // id of token
                Metadata = metadata,
                Amount = tokenAmount,
                SenderAddress = Address,
                ReceiverAddress = receiver
            };

            try
            {
                var rtxid = await NeblioTransactionHelpers.SendNTP1TokenLotWithPaymentAPIAsync(dto, AccountKey, neblioAmount, res.Item2, tres.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Airdrop Sent");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        /// <summary>
        /// Add comment to Coruzant Post NFT or add comment and send it to new owner
        /// </summary>
        /// <param name="receiver">Fill when you want to send to different address</param>
        /// <param name="NFT"></param>
        /// <param name="commentWrite">Set this if you need to just write the comment to the NFT</param>
        /// <param name="comment">Add your comment</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendCoruzantNFT(string receiver, INFT NFT, bool commentWrite, string comment = "")
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot snd NFT without provided Utxo TxId.", "Cannot send NFT");
                return (false, "Cannot send NFT without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enough spendable Neblio inputs");
                return (false, res.Item1);
            }

            if (string.IsNullOrEmpty(receiver))
                receiver = string.Empty;

            try
            {
                var rtxid = await CoruzantNFTHelpers.ChangeCoruzantPostNFT(Address, AccountKey, nft, res.Item2, receiver);

                if (rtxid != null)
                {
                    if (!commentWrite)
                        await InvokeSendPaymentSuccessEvent(rtxid, "NFT Sent");
                    else
                        await InvokeSendPaymentSuccessEvent(rtxid, "Comment written to NFT");
                    return (true, rtxid);
                }
            }
            catch (Exception ex)
            {
                await InvokeErrorDuringSendEvent(ex.Message, "Unknown Error");
                return (false, ex.Message);
            }

            await InvokeErrorDuringSendEvent("Unknown Error", "Unknown Error");
            return (false, "Unexpected error during send.");
        }

        #endregion

        #region NFTIoTDevicesProcessing
        /// <summary>
        /// Init NFT IoT Devices automatically if they have allowed this function
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, string)> InitAllAutoIoTDeviceNFT()
        {
            try
            {
                NFTs.Where(n => n.Type == NFTTypes.IoTDevice).ToList()?.ForEach(async (nft) =>
                {
                    if ((nft as IoTDeviceNFT).AutoActivation && !(nft as IoTDeviceNFT).Active)
                        await InitIoTDeviceNFT(nft.Utxo, nft.UtxoIndex);
                });
                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Init IoT Device manually
        /// </summary>
        /// <param name="utxo"></param>
        /// <param name="utxoindex"></param>
        /// <returns></returns>
        public async Task<(bool, string)> InitIoTDeviceNFT(string utxo, int utxoindex = 0)
        {
            try
            {
                var nft = NFTs.First(n => n.Type == NFTTypes.IoTDevice && n.Utxo == utxo && n.UtxoIndex == utxoindex);
                if (nft != null && !(nft as IoTDeviceNFT).Active)
                {
                    (nft as IoTDeviceNFT).NewMessage += NeblioAccountBase_NewMessage;
                    await (nft as IoTDeviceNFT).InitCommunication(Secret);
                    Console.WriteLine($"IoT NFT Device {utxo}:{utxoindex} Initialized.");
                }
                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Deactivate the IoT Device
        /// </summary>
        /// <param name="utxo"></param>
        /// <param name="utxoindex"></param>
        /// <returns></returns>
        public async Task<(bool, string)> DeInitIoTDeviceNFT(string utxo, int utxoindex = 0)
        {
            try
            {
                var nft = NFTs.First(n => n.Type == NFTTypes.IoTDevice && n.Utxo == utxo && n.UtxoIndex == utxoindex);
                if (nft != null && (nft as IoTDeviceNFT).Active)
                {
                    (nft as IoTDeviceNFT).NewMessage -= NeblioAccountBase_NewMessage;
                    await (nft as IoTDeviceNFT).DeInitCommunication();
                    Console.WriteLine($"IoT NFT Device {utxo}:{utxoindex} Deinitialized.");
                }
                return (true, "OK");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private void NeblioAccountBase_NewMessage(object sender, (string, INFT) e)
        {
            var n = e.Item2;
            Console.WriteLine("New Message received from the IoTDevice to main.");
            MintNFTMessageForIoTDeviceEvent((sender as IoTDeviceNFT).Utxo, n.Name, n.Description, n, e.Item1, (sender as IoTDeviceNFT));
        }

        private async Task MintNFTMessageForIoTDeviceEvent(string senderUtxo, string name, string message, INFT nft, string messagekey, IoTDeviceNFT sender)
        {
            if (TotalSpendableBalance > 0.01)
            {
                var receiver = sender.ReceivingMessageAddress;
                if (string.IsNullOrEmpty(receiver)) receiver = Address;
                var res = await SendIoTMessageNFT(nft, Address, receiver);
                if (res.Item1)
                {
                    Console.WriteLine($"NFT Message from IoTDeviceNFT {senderUtxo} was minted OK. New Tx Hash is: {res.Item2}.");
                    sender.MarkMessageAsProcessed(messagekey);
                }
                else
                    Console.WriteLine($"Cannot mint the NFT: {res.Item2}");
            }
        }

        #endregion

        #region NFTPaymentsProcessing

        /// <summary>
        /// This function will check payments and try to find them complementary NFT which was sold. 
        /// If there is price mathc and enough of confirmations it will try to send NFT to new owner.
        /// </summary>
        /// <returns></returns>
        public async Task CheckPayments()
        {
            try
            {
                List<INFT> pnfts = null;
                lock (_lock)
                {
                    pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
                }
                if (pnfts != null && pnfts.Count > 0)
                {
                    pnfts.Reverse();
                    foreach (var p in pnfts)
                    {
                        if (!((PaymentNFT)p).AlreadySoldItem && !((PaymentNFT)p).Returned)
                        {
                            var txinfo = await NeblioTransactionHelpers.GetTransactionInfo(p.Utxo);
                            if (txinfo != null && txinfo.Confirmations > NeblioTransactionHelpers.MinimumConfirmations)
                            {
                                INFT pn = null;
                                lock (_lock)
                                {
                                    pn = NFTs.FirstOrDefault(n => (n.Utxo == ((PaymentNFT)p).NFTUtxoTxId && n.UtxoIndex == ((PaymentNFT)p).UtxoIndex));
                                }

                                if (pn != null)
                                {
                                    Console.WriteLine($"Payment for NFT {pn.Name} received and it will be processed.");
                                    Console.WriteLine($"Received amount {p.Price} NEBL.");
                                    Console.WriteLine($"Requested amount {pn.Price} NEBL.");

                                    if (pn != null && pn.Price > 0 && p.Price >= pn.Price)
                                    {
                                        try
                                        {
                                            var res = await CheckSpendableNeblio(0.001);
                                            if (res.Item2 != null)
                                            {
                                                var rtxid = string.Empty;
                                                var pntosend = await NFTFactory.CloneNFT(pn);
                                                if (!pn.SellJustCopy)
                                                    rtxid = await NFTHelpers.SendOrderedNFT(Address, AccountKey, (PaymentNFT)p, pntosend, res.Item2);
                                                else
                                                    rtxid = await NFTHelpers.SendOrderedNFTCopy(Address, AccountKey, (PaymentNFT)p, pntosend, res.Item2);
                                                
                                                if (!string.IsNullOrEmpty(rtxid))
                                                {
                                                    Console.WriteLine($"NFT sent to the buyer {((PaymentNFT)p).Sender} with txid: {rtxid}");
                                                    lock (_lock)
                                                    {
                                                        NFTs.Remove(p); // remove sent payment
                                                        if (!pn.SellJustCopy)
                                                            NFTs.Remove(pn);
                                                    }
                                                    /*
                                                    await Task.Delay(500);
                                                    await ReloadUtxos();
                                                    await ReLoadNFTs();
                                                    await RefreshAddressReceivedPayments();
                                                    await RefreshAddressReceivedReceipts();
                                                    */
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine("You do not have spendable utxo for the fee or NFT is not spendable yet.");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //await InvokeErrorDuringSendEvent($"Cannot send ordered NFT. Payment TxId: {p.Utxo}, NFT TxId: {pn.Utxo}, error message: {ex.Message}", "Cannot send ordered NFT");
                                            Console.WriteLine("Cannot send ordered NFT, payment txid: " + p.Utxo + " - " + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Price in the NFT Payment {p.NFTOriginTxId} does not match with the found NFT {pn.Utxo}.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Cannot find the NFT for the received Payment. Marking the NFT Payment as already sold item.");
                                    try
                                    {
                                        //TODO:
                                        var off = false;
                                        if (off)
                                        {
                                            PaymentNFT pn2send = p as PaymentNFT;
                                            pn2send.AlreadySoldItem = true;
                                            pn2send.OriginalPaymentTxId = p.Utxo;
                                            var res = await SendNFT(Address, pn2send, priceWrite: true, price: p.Price);
                                            if (res.Item1)
                                            {
                                                Console.WriteLine($" NFT Payment {p.Name} was marked as already sold. Please inform sender about it and return payment.");
                                                Console.WriteLine($" NFT Payment processed in txid: {res.Item2}");
                                                Console.WriteLine(res);
                                                lock (_lock)
                                                {
                                                    NFTs.Remove(p); // remove sent payment
                                                }
                                                /*
                                                await Task.Delay(1000);
                                                await ReloadUtxos();
                                                await ReLoadNFTs();
                                                await RefreshAddressReceivedPayments();
                                                await RefreshAddressReceivedReceipts();
                                                */
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Cannot mark the NFT Payment {p.Name} with txid: {p.Utxo} with index: {p.UtxoIndex} as already sold. Error: " + ex.Message);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Cannot process the NFT Payment {p.Name} with txid: {p.Utxo} with index: {p.UtxoIndex}. Waiting for enough confirmations.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot Check the payments. " + ex.Message);
            }
        }

        #endregion
    }
}
