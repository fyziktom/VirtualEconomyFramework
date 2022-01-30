using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Accounts.Dto;
using VEDriversLite.Cryptocurrencies;
using VEDriversLite.Events;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;
using Dasync.Collections;

namespace VEDriversLite.Accounts
{
    public class NeblioAccount : CommonAccount, INFTAccount
    {
        private static object _lock { get; set; } = new object();


        /// <summary>
        /// Total balance of VENFT tokens which can be used for minting purposes.
        /// </summary>
        public double SourceTokensBalance { get; set; } = 0.0;
        /// <summary>
        /// Total number of NFT on the address. It counts also Profile NFT, etc.
        /// </summary>
        public int AddressNFTCount { get; set; } = 0;
        /// <summary>
        /// Limit the number of the loaded NFTs on the address
        /// if it is 0 it will load all of them
        /// </summary>
        public int MaximumOfLoadedNFTs { get; set; } = 0;

        /// <summary>
        /// List of actual address NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        public List<INFT> NFTs
        {
            get
            {
                return NFTsDict.Values.OrderBy(n => n.Time)?.Reverse()?.ToList();
            }
        }
        /// <summary>
        /// Dictionary of actual address NFTs. Based on Utxos list
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> NFTsDict { get; set; } = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// Received payments (means Payment NFT) of this address.
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments { get; set; } = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// Received receipts (means Receipt NFT) of this address.
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedReceipts { get; set; } = new ConcurrentDictionary<string, INFT>();
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
        /// Actual loaded address info. It has inside list of all transactions.
        /// </summary>
        [JsonIgnore]
        public GetAddressResponse AddressInfo { get; set; } = new GetAddressResponse();

        /// <summary>
        /// This event is fired whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public override event EventHandler Refreshed;
        /// <summary>
        /// This event is fired whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public override event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        public override event EventHandler<string> NFTsChanged;

        /// <summary>
        /// This event is fired whenever some progress during multimint happens
        /// </summary>
        public override event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is fired whenever price from exchanges is refreshed. It provides dictionary of the actual available rates.
        /// </summary>
        public override event EventHandler<IDictionary<CurrencyTypes, double>> PricesRefreshed;

        /// <summary>
        /// This event is fired whenever profile nft is updated or found
        /// </summary>
        public override event EventHandler<INFT> ProfileUpdated;
        /// <summary>
        /// This event is fired whenever some NFT is in received payment too and it should be blocked for any further action.
        /// It provides Utxo and UtxoIndex as touple.
        /// Event is fired also for the SubAccounts when it is registred from Main Account
        /// </summary>
        public override event EventHandler<(string, int)> NFTAddedToPayments;
        /// <summary>
        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        public override event EventHandler<string> FirsLoadingStatus;
        /// <summary>
        /// This event is called when first loading of the account is finished
        /// </summary>
        public override event EventHandler<string> AccountFirsLoadFinished;

        public override Task<(bool, string)> AddTxToSendBuffer(TxToSend txtosend)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This function will check if there is some spendable neblio of specific amount and returns list of the utxos for the transaction
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public override async Task<(string, ICollection<Utxo>)> CheckSpendableMainToken(double amount)
        {
            try
            {
                var nutxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(Address, 0.0002, amount);
                if (nutxos == null || nutxos.Count == 0)
                    return ($"You dont have Neblio on the address. Probably waiting for more than {NeblioTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                {
                    var nxs = new List<Utxo>();
                    foreach (var n in nutxos)
                    {
                        nxs.Add(new Utxo()
                        {
                            Txid = n.Txid,
                            Index = (int)n.Index,
                            Blockheight = (double)n.Blockheight,
                            Script = n.ScriptPubKey.Asm,
                            Time = (double)n.Blocktime,
                            Value = (double)n.Value
                        });
                    }
                    return ("OK", nxs);
                }
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
        public override async Task<(string, ICollection<Utxo>)> CheckSpendableTokens(string id, int amount)
        {
            try
            {
                var tutxos = await NeblioTransactionHelpers.FindUtxoForMintNFT(Address, id, amount);
                if (tutxos == null || tutxos.Count == 0)
                    return ($"You dont have Tokens on the address. You need at least 5 for minting. Probably waiting for more than {NeblioTransactionHelpers.MinimumConfirmations} confirmations.", null);
                else
                {
                    var txs = await GetUtxoListFromAPIRelated(tutxos);
                    if (txs.Item1)
                        return ("OK", txs.Item2);
                    else
                        return ("Cannot get Utxo List.", txs.Item2);
                }
            }
            catch (Exception ex)
            {
                return ("Cannot check spendable Neblio Tokens. " + ex.Message, null);
            }
        }

        /// <summary>
        /// Convert the Utxos list obtained from specified API of the blockchain to the common one
        /// </summary>
        /// <param name="neblioUtxos"></param>
        /// <returns></returns>
        private async Task<(bool, List<Utxo>)> GetUtxoListFromAPIRelated(List<NeblioAPI.Utxos> neblioUtxos)
        {
            try
            {
                var txs = new List<Utxo>();
                foreach (var n in neblioUtxos)
                {
                    var t = new Utxo()
                    {
                        Txid = n.Txid,
                        Index = (int)n.Index,
                        Blockheight = (double)n.Blockheight,
                        AccountType = AccountType.Neblio,
                        Script = n.ScriptPubKey.Asm,
                        Address = Address,
                        Time = (double)n.Blocktime,
                        Value = (double)n.Value,
                    };

                    n.Tokens?.ToList()?
                        .ForEach(tk => t.Tokens.Add(new Token()
                        {
                            TokenId = tk.TokenId,
                            Amount = (double)tk.Amount,
                            IssueTxId = tk.IssueTxid
                        }));

                    txs.Add(t);
                }
                return (true, txs);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot create Utxo list from Neblio Utxos list. " + ex.Message);
            }
            return (false, new List<Utxo>());
        }

        public override async Task<(bool,string)> StartRefreshingData(int interval = 3000) 
        {
            try
            {
                FirsLoadingStatus?.Invoke(this, "Loading of address data started.");

                AddressInfo = new GetAddressResponse();
                AddressInfo.Transactions = new List<string>();

                await ReloadUtxos();
                await ExchangePriceService.InitPriceService(Cryptocurrencies.ExchangeRatesAPITypes.Coingecko,
                                                            Address,
                                                            Cryptocurrencies.CurrencyTypes.NEBL);

                FirsLoadingStatus?.Invoke(this, "Utxos loaded");
                Refreshed?.Invoke(this, null);

                if (!WithoutNFTs)
                {
                    FirsLoadingStatus?.Invoke(this, "Loading NFTs started.");

                    await ReLoadNFTs(true, maxItems: MaximumOfLoadedNFTs, firstLoad:true);

                    var tasks = new Task[2];
                    tasks[0] = RefreshAddressReceivedPayments();
                    tasks[1] = RefreshAddressReceivedReceipts();
                    await Task.WhenAll(tasks);

                    RegisterPriceServiceEventHandler();

                    Refreshed?.Invoke(this, null);
                    FirsLoadingStatus?.Invoke(this, "Main Account NFTs Loaded.");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during start of the load of the Account. " + ex.Message);
            }
            var minorRefresh = 2;
            var firstLoad = true;
            AccountFirsLoadFinished?.Invoke(Address, "OK");

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                IsRefreshingRunning = true;
                var tasks = new Task[2];

                while (true)
                {
                    try
                    {
                        if (!firstLoad)
                        {
                            await ReloadUtxos();

                            if (!WithoutNFTs)
                            {
                                await ReLoadNFTs(true, maxItems: MaximumOfLoadedNFTs);

                                tasks[0] = RefreshAddressReceivedPayments();
                                tasks[1] = RefreshAddressReceivedReceipts();
                                await Task.WhenAll(tasks);
                            }

                            Refreshed?.Invoke(Address, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception in neblio main loop. " + ex.Message);
                    }

                    if (firstLoad)
                    {
                        await Task.Delay(interval * 5);
                        firstLoad = false;
                    }
                    else
                        await Task.Delay(interval);
                }
            });

            return (true, "OK");

        }


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

        /////////////////////////////////////
        //Create or Load Account section
        #region LoadAccount

        public override async Task<bool> LoadAccount(AccountLoadData loaddata)
        {
            try
            {
                WithoutNFTs = loaddata.LoadWithoutNFTs;
                if (loaddata.LoadFromFile && !string.IsNullOrEmpty(loaddata.Filename))
                {
                    if (FileHelpers.IsFileExists(loaddata.Filename) && loaddata.LoadFromFile)
                    {
                        try
                        {
                            var k = FileHelpers.ReadTextFromFile(loaddata.Filename);
                            var kdto = JsonConvert.DeserializeObject<KeyDto>(k);
                            loaddata.Key = kdto.Key;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
                        }
                    }
                    else
                    {
                        // cannot find the file with the key
                        return false;
                    }
                }

                if (!loaddata.LoadJustToObserve)
                {
                    // load with correct key
                    if (await LoadAccountKey(loaddata.Password, loaddata.Key))
                    {
                        if (loaddata.LoadJustForSignitures)
                            WithoutNFTs = true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // Load with dummy key just for the observation
                    // It cannot sign the transactions
                    Secret = new Key().GetBitcoinSecret(NeblioTransactionHelpers.Network);
                    AccountKey = new EncryptionKey(Secret.ToString(), string.Empty, fromDb: false);
                    Address = loaddata.Address;
                }

                SignMessage("init");
                if (!IsRefreshingRunning)
                    await StartRefreshingData();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot load the account. " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// This function will create new account - Neblio address and its Private key.
        /// </summary>
        /// <param name="password">Input password, which will encrypt the Private key</param>
        /// <param name="saveToFile">if you want to save it to the file (dont work in the WASM) set this. It will save to root exe path as key.txt</param>
        /// <param name="filename">default filename is key.txt you can change it, but remember to load same name when loading the account.</param>
        /// <returns></returns>
        public override async Task<bool> CreateNewAccount(string password, bool saveToFile = false, string filename = "key.txt")
        {
            try
            {
                Key privateKey = new Key(); // generate a random private key
                PubKey publicKey = privateKey.PubKey;
                BitcoinSecret privateKeyFromNetwork = privateKey.GetBitcoinSecret(NeblioTransactionHelpers.Network);
                var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
                Address = address.ToString();

                // todo load already encrypted key
                AccountKey = new Security.EncryptionKey(privateKeyFromNetwork.ToString(), password);
                AccountKey.PublicKey = Address;
                Secret = privateKeyFromNetwork;
                if (!string.IsNullOrEmpty(password))
                    AccountKey.PasswordHash = await Security.SecurityUtils.HashPassword(password);
                
                if (saveToFile)
                {
                    // save to file
                    var kdto = new KeyDto()
                    {
                        Address = Address,
                        Key = await AccountKey.GetEncryptedKey(returnEncrypted: true)
                    };
                    FileHelpers.WriteTextToFile(filename, JsonConvert.SerializeObject(kdto));
                }

                SignMessage("init");

                await StartRefreshingData();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot Create Account. " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Load account Key. If there is password it is used to decrypt the private key
        /// This function will load Secret property with Key
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <returns></returns>
        public async Task<bool> LoadAccountKey(string password, string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(password))
                    AccountKey = new EncryptionKey(key, fromDb: true);
                else
                    AccountKey = new EncryptionKey(key, fromDb: false);
                if (!string.IsNullOrEmpty(password))
                    await AccountKey.LoadPassword(password);

                Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), NeblioTransactionHelpers.Network);
                SignMessage("init");

                var add = await NeblioTransactionHelpers.GetAddressFromPrivateKey(Secret.ToString());
                if (add.Item1) Address = add.Item2;

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot load the Key for the account {Address}. " + ex.Message);
            }
        }

        #endregion
        /////////////////////////////////////

        /////////////////////////////////////
        // Load of Utxos and NFTs
        #region LoadingOfAccountInfoAndNFTs

        /// <summary>
        /// This function will load actual address info an adress utxos. It is used mainly for loading list of all transactions.
        /// </summary>
        /// <returns></returns>
        public override async Task ReloadAccountInfo()
        {
            var ai = await NeblioTransactionHelpers.AddressInfoAsync(Address);

            if (ai != null)
            {
                lock (_lock)
                {
                    AddressInfo = ai;
                    if (AddressInfo != null)
                        AddressInfo.Transactions = AddressInfo.Transactions.Reverse().ToList();
                    else
                        AddressInfo = new GetAddressResponse();
                }
            }
        }

        /// <summary>
        /// Reload address Utxos list. It will sort descending the utxos based on the utxos time stamps.
        /// </summary>
        /// <returns></returns>
        public override async Task ReloadUtxos()
        {
            var ux = await NeblioTransactionHelpers.GetAddressUtxosObjects(Address);
            if (ux == null)
                return;

            var ouxox = ux.ToList();
            if (ouxox.Count > 0)
            {
                var tos = await NeblioTransactionHelpers.CheckTokensSupplies(Address, new GetAddressInfoResponse() { Utxos = ux });  
                var mintingSupply = await NeblioTransactionHelpers.GetActualMintingSupply(Address, NFTHelpers.TokenId, new GetAddressInfoResponse() { Utxos = ux });
                
                lock (_lock)
                {
                    SourceTokensBalance = mintingSupply.Item1;
                    TokensSupplies = tos;
                }
                
                var uxs = await GetUtxoListFromAPIRelated(ouxox);
                lock (_lock)
                {
                    Utxos.Clear();
                    TotalBalance = 0.0;
                    TotalUnconfirmedBalance = 0.0;
                    TotalSpendableBalance = 0.0;
                    if (!uxs.Item1)
                        return;

                    uxs.Item2.ForEach(u =>
                    {
                        if (u.Blockheight <= 0)
                        {
                            TotalUnconfirmedBalance += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);
                        }
                        else
                        {
                            TotalSpendableBalance += ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio);
                            Utxos.Add(u);
                        }
                    });

                    TotalBalance = TotalSpendableBalance + TotalUnconfirmedBalance;
                }
            }
        }

        /// <summary>
        /// Load or reload the NFTs for this account.
        /// It will add new to the NFTsDict and also remove old ones
        /// </summary>
        /// <param name="fireProfileEvent">Fire profile event when you will find it between loaded NFTs</param>
        /// <param name="withoutMessages">Dont load the messages</param>
        /// <param name="justMessages">Load just the messages</param>
        /// <param name="maxItems">Limist maximum items to load</param>
        /// <param name="firstLoad">first load will not fire NFTsChanged event after load of all NFTs, but after whole list</param>
        /// <returns></returns>
        public async Task ReLoadNFTs(bool fireProfileEvent = false, bool withoutMessages = false, bool justMessages = false, int maxItems = 0, bool firstLoad = false)
        {
            if (string.IsNullOrEmpty(Address))
                return;

            try
            {
                var nftutxos = Utxos.Where(u => (u.Value == 10000 && u.Tokens.Count > 0)).Where(u => u.Tokens[0]?.Amount == 1).ToArray();
                if (nftutxos == null || nftutxos.Count() == 0)
                    return;

                var fireProfileEventTmp = true;
                var lastnftsCount = NFTsDict.Count();
                ArraySegment<Utxo> nftus = null;
                if (maxItems > 0)
                    nftus = new ArraySegment<Utxo>(nftutxos, 0, maxItems);
                else
                    nftus = nftutxos;

                var nftutxosCount = nftus.Count();

                if (NFTsDict.Count > 0)
                {
                    // remove old ones
                    foreach (var n in NFTsDict.Values)
                        if (nftus.FirstOrDefault(nu => nu.Txid == n.Utxo && nu.Index == n.UtxoIndex) == null)
                        {
                            NFTsDict.TryRemove($"{n.Utxo}:{n.UtxoIndex}", out var nft);
                            NFTsChanged?.Invoke(this, "Changed");
                        }
                }

                await nftus.ParallelForEachAsync(async n =>
                {
                    if (maxItems == 0 || (maxItems > 0 && NFTsDict.Count < maxItems))
                    {
                        var tok = n.Tokens.FirstOrDefault();
                        if (!NFTsDict.TryGetValue($"{n.Txid}:{n.Index}", out var nft))
                        {
                            if (!withoutMessages)
                                nft = await NFTFactory.GetNFT(tok.TokenId, n.Txid, n.Index, n.Time, address: Address);
                            else if (withoutMessages)
                                nft = await NFTFactory.GetNFT(tok.TokenId, n.Txid, n.Index, n.Time, address: Address, skipTheType: true, skipType: NFTTypes.Message);
                            if (nft != null)
                            {
                                if (fireProfileEventTmp && fireProfileEvent && nft.Type == NFTTypes.Profile)
                                {
                                    ProfileUpdated?.Invoke(Address, nft);
                                    fireProfileEventTmp = false;
                                }

                                NFTsDict.TryAdd($"{n.Txid}:{n.Index}", nft);

                                var count = maxItems > 0 ? maxItems : nftutxosCount;
                                FirsLoadingStatus?.Invoke(Address, $"Loaded {NFTsDict.Count} NFT of {count}.");
                                if (!firstLoad)
                                    NFTsChanged?.Invoke(this, "Changed");
                            }
                        }
                    }
                }, maxDegreeOfParallelism: 10);

                if (firstLoad && NFTsDict.Count != lastnftsCount)
                    NFTsChanged?.Invoke(this, "Changed");

                AddressNFTCount = NFTsDict.Count;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reload NFTs. " + ex.Message);
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

                    if ((firstpnft != null && ffirstpnft != null) || (firstpnft == null && ffirstpnft != null))
                    {
                        if ((firstpnft == null && ffirstpnft != null) || (firstpnft != null && (firstpnft.Utxo != ffirstpnft.Utxo)))
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

        public void FireNFTAddedToPayments(string address, (string, int) e)
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
        /////////////////////////////////////

        /////////////////////////////////////
        //Functions related to the NFT Cache
        #region NFTCache

        /// <summary>
        /// Serialize the NFTCache dictionary
        /// </summary>
        /// <returns></returns>
        public async Task<string> CacheNFTs()
        {
            try
            {
                var output = string.Empty;
                var nout = new Dictionary<string, NFTCacheDto>();

                lock (_lock)
                {
                    foreach (var n in VEDLDataContext.NFTCache)
                    {
                        if ((DateTime.UtcNow - n.Value.LastAccess) < new TimeSpan(10, 0, 0, 0))
                            nout.Add(n.Key, n.Value);
                    }
                    output = JsonConvert.SerializeObject(nout);
                }
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine("cannot cache the NFTs. " + ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Load the data from the stirng to the Dictionary of the NFTs cache
        /// The input string must be serialized NFTCache dictionary from VEDriversLite with use of the function CacheNFTs from this class
        /// </summary>
        /// <param name="cacheString">Input serialized NFTCache dictionary as string</param>
        /// <returns></returns>
        public async Task<bool> LoadCacheNFTsFromString(string cacheString)
        {
            if (string.IsNullOrEmpty(cacheString)) return false;
            try
            {
                var nfts = JsonConvert.DeserializeObject<ConcurrentDictionary<string, NFTCacheDto>>(cacheString);
                if (nfts != null)
                {
                    lock (_lock)
                    {
                        VEDLDataContext.NFTCache = nfts;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load NFT cache to the dictionary. " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Load the NFTCache data from the input dictionary to the Dictionary of the NFTs cache
        /// The input must be dictionary which contains NFTCacheDto as value with cache data
        /// </summary>
        /// <param name="nfts">Input NFTCache dictionary</param>
        /// <returns></returns>
        public async Task<bool> LoadCacheNFTsFromString(IDictionary<string, NFTCacheDto> nfts)
        {
            try
            {
                if (nfts != null)
                {
                    lock (_lock)
                    {
                        VEDLDataContext.NFTCache.Clear();
                        foreach (var n in nfts)
                            VEDLDataContext.NFTCache.TryAdd(n.Key, n.Value);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load NFT cache to the dictionary. " + ex.Message);
                return false;
            }
        }

        #endregion
        /////////////////////////////////////
    }
}
