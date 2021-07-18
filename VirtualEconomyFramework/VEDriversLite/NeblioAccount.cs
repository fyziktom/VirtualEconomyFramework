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
using VEDriversLite.Dto;
using VEDriversLite.Events;
using VEDriversLite.Messaging;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.Security;
using VEDriversLite.WooCommerce;

namespace VEDriversLite
{
    public class NeblioAccount
    {
        public NeblioAccount()
        {
            Profile = new ProfileNFT("");
            NFTs = new List<INFT>();
            Bookmarks = new List<Bookmark>();
            InitHandlers();
            NFTHelpers.InitNFTHelpers();
        }

        private static object _lock { get; set; } = new object();

        public string MessagingTokensId { get; } = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
        /// <summary>
        /// Neblio Address hash
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Loaded Secret, NBitcoin Class which carry Public Key and Private Key
        /// </summary>
        public BitcoinSecret Secret { get; set; }

        /// <summary>
        /// Number of the transactions on the address. not used now
        /// </summary>
        public double NumberOfTransaction { get; set; } = 0;
        /// <summary>
        /// Number of already loaded transaction on the address. not used now
        /// </summary>
        public double NumberOfLoadedTransaction { get; } = 0;
        /// <summary>
        /// If the address has enought Neblio to buy source VENFT tokens (costs 1 NEBL) this is set as true.
        /// </summary>
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
        public double CoruzantSourceTokensBalance { get; set; } = 0.0;
        /// <summary>
        /// Total number of NFT on the address. It counts also Profile NFT, etc.
        /// </summary>
        public double AddressNFTCount { get; set; } = 0.0;
        /// <summary>
        /// When main refreshing loop is running this is set
        /// </summary>
        public bool IsRefreshingRunning { get; set; } = false;
        /// <summary>
        /// If there is some Doge address in same project which should be searched for the payments triggers fill it here
        /// </summary>
        public string ConnectedDogeAccountAddress { get; set; } = string.Empty;
        public string LastCheckedDogePaymentUtxo { get; set; } = string.Empty;
        /// <summary>
        /// List of actual address NFTs. Based on Utxos list
        /// </summary>
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// List of actual address Coruzant NFTs. Based on Utxos list
        /// </summary>
        public List<INFT> CoruzantNFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// List of all active tabs for browsing or interacting with the address. All has possibility to load own list of NFTs.
        /// </summary>
        public List<ActiveTab> Tabs { get; set; } = new List<ActiveTab>();
        public List<MessageTab> MessageTabs { get; set; } = new List<MessageTab>();
        /// <summary>
        /// Neblio Sub Accounts. Each can work with own set of NFTs. It is real blockchain address with own Private Key
        /// </summary>
        public Dictionary<string, NeblioSubAccount> SubAccounts { get; set; } = new Dictionary<string, NeblioSubAccount>();
        /// <summary>
        /// Received payments (means Payment NFT) of this address.
        /// </summary>
        public ConcurrentDictionary<string, INFT> ReceivedPayments = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// If address has some profile NFT, it is founded in Utxo list and in this object.
        /// </summary>
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");
        /// <summary>
        /// Actual all token supplies. Consider also other tokens than VENFT.
        /// </summary>
        public Dictionary<string, TokenSupplyDto> TokensSupplies { get; set; } = new Dictionary<string, TokenSupplyDto>();
        /// <summary>
        /// Actual list of all Utxos on this address.
        /// </summary>
        public List<Utxos> Utxos { get; set; } = new List<Utxos>();
        /// <summary>
        /// List of all saved bookmarks. This is just realtime carrier. It need some serialization/deserialization.
        /// </summary>
        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        /// <summary>
        /// Actual loaded address info. It has inside list of all transactions.
        /// </summary>
        public GetAddressResponse AddressInfo { get; set; } = new GetAddressResponse();
        /// <summary>
        /// Actual loaded address info with list of Utxos. When utxos are loaded first, this is just fill with it to prevent not necessary API request.
        /// </summary>
        public GetAddressInfoResponse AddressInfoUtxos { get; set; } = new GetAddressInfoResponse();

        public CoruzantBrowser CoruzantBrowserInstance { get; set; } = new CoruzantBrowser();

        /// <summary>
        /// This event is called whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;
        /// <summary>
        /// This event is called whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// This event is called whenever the list of NFTs is changed
        /// </summary>
        public event EventHandler<string> NFTsChanged;

        /// <summary>
        /// This event is called whenever some progress during multimint happens
        /// </summary>
        public event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is called whenever profile nft is updated or found
        /// </summary>
        public event EventHandler<INFT> ProfileUpdated;

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

        /// <summary>
        /// This function will register event info from NeblioTransactionHelpers class.
        /// </summary>
        private void InitHandlers()
        {
            NeblioTransactionHelpers.NewEventInfo += NeblioTransactionHelpers_NewEventInfo;
        }
        /// <summary>
        /// This function will unregister event info from NeblioTransactionHelpers class.
        /// </summary>
        private void DeInitHandlers()
        {
            NeblioTransactionHelpers.NewEventInfo -= NeblioTransactionHelpers_NewEventInfo;
        }
        /// <summary>
        /// Handler for event info messages. Now it just store events in common store.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NeblioTransactionHelpers_NewEventInfo(object sender, IEventInfo e)
        {
            e.Address = Address;
            EventInfoProvider.StoreEventInfo(e);
        }

        /// <summary>
        /// Invoke Success message info event
        /// </summary>
        /// <param name="txid">new tx id hash</param>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        private async Task InvokeSendPaymentSuccessEvent(string txid, string title = "Neblio Payment Sent")
        {
            NewEventInfo?.Invoke(this,
                        await EventFactory.GetEvent(EventType.Info,
                                                    title,
                                                    $"Successfull send. Please wait a while for enough confirmations.",
                                                    Address,
                                                    txid,
                                                    100));
        }

        /// <summary>
        /// Invoke Error message because account is locked
        /// </summary>
        /// <param name="title">Title of the event message</param>
        /// <returns></returns>
        private async Task InvokeAccountLockedEvent(string title = "Cannot send transaction")
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
        private async Task InvokeErrorDuringSendEvent(string errorMessage, string title = "Cannot send transaction")
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
        private async Task InvokeErrorEvent(string errorMessage, string title = "Error")
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
        /// This function will load the actual data and then run the task which periodically refresh this data.
        /// It doesnt have cancellation now!
        /// </summary>
        /// <param name="interval">Default interval is 3000 = 3 seconds</param>
        /// <returns></returns>
        public async Task<string> StartRefreshingData(int interval = 3000)
        {
            try
            {
                AddressInfo = new GetAddressResponse();
                AddressInfo.Transactions = new List<string>();

                await ReloadUtxos();
                await ReloadMintingSupply();
                await ReloadTokenSupply();
                Refreshed?.Invoke(this, null);
                await ReLoadNFTs(true);
                await ReloadCountOfNFTs();
            }
            catch (Exception ex)
            {
                // todo
            }

            var minorRefresh = 5;
            var firstLoad = true;
            // todo cancelation token
            _ = Task.Run(async () =>
            {
                try
                {
                    await ReloadUtxos();
                    await ReloadMintingSupply();
                    await ReloadTokenSupply();
                    Refreshed?.Invoke(this, null);
                    await ReLoadNFTs(true);
                    await ReloadCountOfNFTs();
                    await CheckPayments();
                    await RefreshAddressReceivedPayments();

                }
                catch (Exception ex)
                {
                    // todo
                }
                IsRefreshingRunning = true;

                while (true)
                {
                    try
                    {
                        //await ReloadAccountInfo();

                        await ReloadUtxos();
                        await ReloadMintingSupply();
                        await ReLoadNFTs();
                        await ReloadCountOfNFTs();
                        await ReloadTokenSupply();

                        try
                        {
                            if (Utxos.FirstOrDefault(u => u != null && u.Txid == Profile.Utxo && u.Index == Profile.UtxoIndex) == null)
                            {
                                Profile = await NFTHelpers.FindProfileNFT(NFTs);
                                if (!string.IsNullOrEmpty(Profile.Utxo))
                                    ProfileUpdated?.Invoke(this, Profile);
                            }
                        }
                        catch (Exception ex)
                        {
                            //todo
                        }
                        minorRefresh--;
                        if (minorRefresh < 0)
                        {
                            await CheckPayments();
                            if (!string.IsNullOrEmpty(ConnectedDogeAccountAddress))
                                await CheckDogePayments();

                            await RefreshAddressReceivedPayments();
                            minorRefresh = 10;
                        }

                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception in nebmlio main loop. " + ex.Message);
                        //await InvokeErrorEvent(ex.Message, "Unknown Error During Refreshing Data");
                    }

                    await Task.Delay(interval);
                }
                IsRefreshingRunning = false;
            });

            return await Task.FromResult("RUNNING");
        }

        /// <summary>
        /// This function will create new account - Neblio address and its Private key.
        /// </summary>
        /// <param name="password">Input password, which will encrypt the Private key</param>
        /// <param name="saveToFile">if you want to save it to the file (dont work in the WASM) set this. It will save to root exe path as key.txt</param>
        /// <returns></returns>
        public async Task<bool> CreateNewAccount(string password, bool saveToFile = false, string filename = "key.txt")
        {
            try
            {
                await Task.Run(async () =>
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
                    SignMessage("init");

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
                });

                await StartRefreshingData();

                return true;
            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot Create Account");
            }

            return false;
        }

        /// <summary>
        /// Load account from filename (default "key.txt") file placed in the root exe directory. Doesnt work in WABS
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password, string filename = "key.txt")
        {
            if (FileHelpers.IsFileExists(filename))
            {
                try
                {
                    var k = FileHelpers.ReadTextFromFile(filename);
                    var kdto = JsonConvert.DeserializeObject<KeyDto>(k);

                    if (!string.IsNullOrEmpty(password))
                    {
                        AccountKey = new EncryptionKey(kdto.Key, fromDb: true);
                    }
                    else
                    {
                        AccountKey = new EncryptionKey(kdto.Key, fromDb: false);
                    }
                    if (!string.IsNullOrEmpty(password))
                    {
                        await AccountKey.LoadPassword(password);
                        AccountKey.IsEncrypted = true;
                    }
                    Address = kdto.Address;

                    Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), NeblioTransactionHelpers.Network);
                    SignMessage("init");

                    if (!IsRefreshingRunning)
                        await StartRefreshingData();

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
                }
            }
            else
            {
                CreateNewAccount(password);
            }

            return false;
        }

        /// <summary>
        /// Load account from filename (default "key.txt") file placed in the root exe directory. Doesnt work in WABS
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <returns></returns>
        public async Task<bool> LoadAccountWithDummyKey(string password, string address)
        {
            try
            {
                Key k = new Key();
                if (!string.IsNullOrEmpty(password))
                {
                    AccountKey = new EncryptionKey(k.ToString(), fromDb: true);
                }
                else
                {
                    AccountKey = new EncryptionKey(k.ToString(), fromDb: false);
                }
                if (!string.IsNullOrEmpty(password))
                {
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;
                }

                Secret = new BitcoinSecret(k.ToString(), NeblioTransactionHelpers.Network);
                Address = address;//Secret.GetAddress(ScriptPubKeyType.Legacy).ToString();

                SignMessage("init");

                if (!IsRefreshingRunning)
                    await StartRefreshingData();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address! " + ex.Message);
            }
        }

        /// <summary>
        /// Load account from filename backup from VENFT App (default "backup.json") file placed in the root exe directory. Doesnt work in WABS
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <returns></returns>
        public async Task<bool> LoadAccountFromVENFTBackup(string password, string fromString = "", string filename = "backup.json")
        {
            if (FileHelpers.IsFileExists(filename) || !string.IsNullOrEmpty(fromString))
            {
                try
                {
                    BackupDataDto bdto = null;
                    if (string.IsNullOrEmpty(fromString))
                    {
                        var k = FileHelpers.ReadTextFromFile(filename);
                        bdto = JsonConvert.DeserializeObject<BackupDataDto>(k);
                    }
                    else
                    {
                        bdto = JsonConvert.DeserializeObject<BackupDataDto>(fromString);
                    }

                    AccountKey = new EncryptionKey(bdto.Key, fromDb: true);
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;
                    if (Address != bdto.Address)
                        Address = bdto.Address;

                    if (!string.IsNullOrEmpty(bdto.DogeAddress))
                    {
                        var dogeacc = new DogeAccount();
                        if (await dogeacc.LoadAccount(password, bdto.DogeKey, bdto.DogeAddress))
                            VEDLDataContext.DogeAccounts.TryAdd(dogeacc.Address, dogeacc);
                    }

                    if (!string.IsNullOrEmpty(bdto.SubAccounts))
                        await LoadSubAccounts(bdto.SubAccounts);
                    if (!string.IsNullOrEmpty(bdto.Bookmarks))
                        await LoadBookmarks(bdto.Bookmarks);
                    if (!string.IsNullOrEmpty(bdto.BrowserTabs))
                        await LoadTabs(bdto.BrowserTabs);

                    Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), NeblioTransactionHelpers.Network);

                    if (!IsRefreshingRunning)
                    {
                        SignMessage("init");
                        await StartRefreshingData();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
                }
            }
            else
            {
                CreateNewAccount(password);
            }

            return false;
        }

        /// <summary>
        /// Load account from password, input encrypted private key and address.
        /// It expect the private key is encrypted by the password.
        /// It uses AES encryption
        /// </summary>
        /// <param name="password"></param>
        /// <param name="encryptedKey"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password, string encryptedKey, string address)
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        AccountKey = new EncryptionKey(encryptedKey, fromDb: true);
                    }
                    else
                    {
                        AccountKey = new EncryptionKey(encryptedKey, fromDb: false);
                    }
                    if (!string.IsNullOrEmpty(password))
                    {
                        await AccountKey.LoadPassword(password);
                        AccountKey.IsEncrypted = true;
                    }
                    Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), NeblioTransactionHelpers.Network);
                    SignMessage("init");
                    Address = address;
                });

                if (!IsRefreshingRunning)
                    await StartRefreshingData();

                return true;
            }
            catch (Exception ex)
            {
                //await InvokeErrorEvent(ex.Message, "Cannot Load Account");
                //throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
            }

            return false;
        }

        #region Bookmarks

        /// <summary>
        /// Load bookmarks from previous serialized list of bookmarks. 
        /// </summary>
        /// <param name="bookmarks"></param>
        /// <returns></returns>
        public async Task LoadBookmarks(string bookmarks)
        {
            try
            {
                var bkm = JsonConvert.DeserializeObject<List<Bookmark>>(bookmarks);
                if (bkm != null)
                    Bookmarks = bkm;
            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot deserialize the bookmarks.");
            }
        }

        /// <summary>
        /// Add new bookmark to bookmark list and return serialized list for save
        /// </summary>
        /// <param name="name">Name of the bookmark. It is important for most functions which work with the bookmarks</param>
        /// <param name="address">Neblio Address</param>
        /// <param name="note">optional note</param>
        /// <returns>Serialized list in string for save</returns>
        public async Task<(bool, string)> AddBookmark(string name, string address, string note)
        {
            if (!Bookmarks.Any(b => b.Address == address))
            {
                var bkm = new Bookmark()
                {
                    Name = name,
                    Address = address,
                    Note = note
                };
                Bookmarks.Add(bkm);
                var tab = Tabs.Find(t => t.Address == address);
                if (tab != null)
                    tab.LoadBookmark(bkm);
                var mt = MessageTabs.Find(t => t.Address == address);
                if (mt != null)
                    mt.LoadBookmark(bkm);
                return (true, JsonConvert.SerializeObject(Bookmarks));
            }
            else
            {
                await InvokeErrorEvent("Bookmark Already Exists", "Already Exists");
                return (false, "Already Exists.");
            }
        }

        /// <summary>
        /// Remove bookmark by the neblio address. It must be found in the bookmark list
        /// </summary>
        /// <param name="address"></param>
        /// <returns>Serialized list in string for save</returns>
        public async Task<(bool, string)> RemoveBookmark(string address)
        {
            var bk = Bookmarks.Find(b => b.Address == address);
            if (bk != null)
                Bookmarks.Remove(bk);
            else
            {
                await InvokeErrorEvent("Bookmark Not Found.", "Not Found");
                return (false, string.Empty);
            }

            var tab = Tabs.Find(t => t.Address == address);
            if (tab != null)
                tab.ClearBookmark();
            var mt = MessageTabs.Find(t => t.Address == address);
            if (mt != null)
                mt.ClearBookmark();
            return (true, JsonConvert.SerializeObject(Bookmarks));
        }

        /// <summary>
        /// Get serialized bookmarks list as string
        /// </summary>
        /// <returns></returns>
        public async Task<string> SerializeBookmarks()
        {
            return JsonConvert.SerializeObject(Bookmarks);
        }

        /// <summary>
        /// Check if the address is already in the bookmarks and return this bookmark
        /// </summary>
        /// <param name="address">Address which should be in the bookmarks</param>
        /// <returns>true and bookmark class if exists</returns>
        public async Task<(bool, Bookmark)> IsInTheBookmarks(string address)
        {
            var bkm = Bookmarks.Find(b => b.Address == address);
            if (bkm == null || string.IsNullOrEmpty(bkm.Address) || string.IsNullOrEmpty(bkm.Name))
                return (false, new Bookmark());
            //return (false, new Bookmark() { Address = NeblioTransactionHelpers.ShortenAddress(address) });
            else
                return (true, bkm);
        }

        #endregion

        #region Tabs
        /// <summary>
        /// Load tabs from previous serialized string.
        /// </summary>
        /// <param name="tabs">List of ActiveTabs as json string</param>
        /// <returns></returns>
        public async Task<string> LoadTabs(string tabs)
        {
            try
            {
                var tbs = JsonConvert.DeserializeObject<List<ActiveTab>>(tabs);
                if (tbs != null)
                    Tabs = tbs;
                var firstAdd = string.Empty;
                if (Tabs.Count > 0)
                {
                    var first = true;
                    foreach (var t in Tabs)
                    {
                        t.Selected = false;
                        var bkm = await IsInTheBookmarks(t.Address);
                        t.LoadBookmark(bkm.Item2);
                        if (first)
                        {
                            await t.Reload();
                            first = false;
                            firstAdd = t.Address;
                        }
                        //else
                        //t.Reload();
                    }
                    Tabs.FirstOrDefault().Selected = true;
                }
                return firstAdd;
            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot deserialize the tabs.");
            }
            return string.Empty;
        }

        /// <summary>
        /// Add new tab based on some Neblio address
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true and string with serialized tabs list as json string</returns>
        public async Task<(bool, string)> AddTab(string address)
        {
            if (!Tabs.Any(t => t.Address == address))
            {
                var bkm = await IsInTheBookmarks(address);
                var tab = new ActiveTab(address);
                tab.BookmarkFromAccount = bkm.Item2;
                tab.Selected = true;

                foreach (var t in Tabs)
                    t.Selected = false;

                await tab.Reload();
                Tabs.Add(tab);
            }
            else
            {
                await InvokeErrorEvent("Tab Already Exists", "Already Exists");
                return (false, "Already Exists.");
            }

            return (true, JsonConvert.SerializeObject(Tabs));
        }

        /// <summary>
        /// Remove tab by Neblio address if exists in the tabs
        /// </summary>
        /// <param name="address">Neblio Address which tab should be removed</param>
        /// <returns>true and string with serialized tabs list as json string</returns>
        public async Task<(bool, string)> RemoveTab(string address)
        {
            var tab = Tabs.Find(t => t.Address == address);
            if (tab != null)
                Tabs.Remove(tab);
            else
            {
                await InvokeErrorEvent("Tab Not Found.", "Not Found");
                return (false, string.Empty);
            }

            foreach (var t in Tabs)
                t.Selected = false;
            Tabs.FirstOrDefault().Selected = true;

            return (true, JsonConvert.SerializeObject(Tabs));
        }
        public async Task SelectTab(string address)
        {
            foreach (var t in Tabs)
                t.Selected = false;
            var tab = Tabs.Find(t => t.Address == address);
            if (tab != null)
                tab.Selected = true;
            if (tab.NFTs.Count == 0)
                await tab.Reload();
        }

        /// <summary>
        /// Return serialized list of ActiveTabs as Json stirng
        /// </summary>
        /// <returns></returns>
        public async Task<string> SerializeTabs()
        {
            return JsonConvert.SerializeObject(Tabs);
        }

        #endregion


        #region MessageTabs
        /// <summary>
        /// Load tabs from previous serialized string.
        /// </summary>
        /// <param name="tabs">List of MessageTabs as json string</param>
        /// <returns></returns>
        public async Task<string> LoadMessageTabs(string tabs)
        {
            try
            {
                var tbs = JsonConvert.DeserializeObject<List<MessageTab>>(tabs);
                if (tbs != null)
                    MessageTabs = tbs;
                var firstAdd = string.Empty;
                if (MessageTabs.Count > 0)
                {
                    var first = true;
                    foreach (var t in MessageTabs)
                    {
                        t.Selected = false;
                        var bkm = await IsInTheBookmarks(t.Address);
                        t.AccountSecret = Secret;
                        t.AccountAddress = Address;
                        t.LoadBookmark(bkm.Item2);
                        if (first)
                        {
                            if (NFTs.Count == 0)
                                ReLoadNFTs();
                            await t.Reload(NFTs);
                            first = false;
                            firstAdd = t.Address;
                        }
                        //else
                        //t.Reload();
                    }
                    MessageTabs.FirstOrDefault().Selected = true;
                }
                return firstAdd;
            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot deserialize the message tabs.");
            }
            return string.Empty;
        }

        /// <summary>
        /// Add new message tab based on some Neblio address
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true and string with serialized tabs list as json string</returns>
        public async Task<(bool, string)> AddMessageTab(string address)
        {
            if (!MessageTabs.Any(t => t.Address == address))
            {
                var bkm = await IsInTheBookmarks(address);
                var tab = new MessageTab(address);
                tab.BookmarkFromAccount = bkm.Item2;
                tab.Selected = true;
                tab.AccountSecret = Secret;
                tab.AccountAddress = Address;

                foreach (var t in MessageTabs)
                    t.Selected = false;

                MessageTabs.Add(tab);
                await tab.Reload(NFTs);
            }
            else
            {
                await InvokeErrorEvent("Message Tab Already Exists", "Already Exists");
                return (false, "Already Exists.");
            }

            return (true, JsonConvert.SerializeObject(MessageTabs));
        }

        /// <summary>
        /// Remove tab by Neblio address if exists in the tabs
        /// </summary>
        /// <param name="address">Neblio Address which tab should be removed</param>
        /// <returns>true and string with serialized tabs list as json string</returns>
        public async Task<(bool, string)> RemoveMessageTab(string address)
        {
            var tab = MessageTabs.Find(t => t.Address == address);
            if (tab != null)
                MessageTabs.Remove(tab);
            else
            {
                await InvokeErrorEvent("Message Tab Not Found.", "Not Found");
                return (false, string.Empty);
            }

            foreach (var t in MessageTabs)
                t.Selected = false;
            MessageTabs.FirstOrDefault().Selected = true;

            return (true, JsonConvert.SerializeObject(MessageTabs));
        }
        public async Task SelectMessageTab(string address)
        {
            foreach (var t in MessageTabs)
                t.Selected = false;
            var tab = MessageTabs.Find(t => t.Address == address);
            if (tab != null)
                tab.Selected = true;
            if (tab.NFTs.Count == 0)
                await tab.Reload(NFTs);
        }

        /// <summary>
        /// Return serialized list of MessageTabs as Json stirng
        /// </summary>
        /// <returns></returns>
        public async Task<string> SerializeMessageTabs()
        {
            return JsonConvert.SerializeObject(MessageTabs);
        }

        #endregion

        #region SubAccounts

        private void Nsa_NewEventInfo(object sender, IEventInfo e)
        {
            NewEventInfo?.Invoke(sender, e);
        }

        /// <summary>
        /// Load subaccounts from previous serialized string.
        /// </summary>
        /// <param name="subaccounts">List of SubAccountsAddressExports as json string</param>
        /// <returns></returns>
        public async Task<string> LoadSubAccounts(string subaccounts)
        {
            SubAccounts.Clear();
            try
            {
                var accnts = JsonConvert.DeserializeObject<List<AccountExportDto>>(subaccounts);

                if (accnts == null)
                    return "Cannot deserialize the data.";

                var firstAdd = string.Empty;
                if (accnts.Count > 0)
                {
                    var first = true;
                    foreach (var a in accnts)
                    {
                        if (!SubAccounts.TryGetValue(a.Address, out var sacc))
                        {
                            var nsa = new NeblioSubAccount();
                            var res = await nsa.LoadFromBackupDto(Secret, a);
                            if (res.Item1)
                            {
                                var bkm = await IsInTheBookmarks(a.Address);
                                if (!bkm.Item1)
                                {
                                    await AddBookmark(a.Name, a.Address, "SubAccount");
                                    bkm = await IsInTheBookmarks(a.Address);
                                }
                                nsa.LoadBookmark(bkm.Item2);
                                nsa.IsAutoRefreshActive = true;
                                nsa.NewEventInfo += Nsa_NewEventInfo;
                                await nsa.StartRefreshingData();
                                nsa.NewMintingProcessInfo += Nsa_NewMintingProcessInfo;
                                nsa.NFTsChanged += Nsa_NFTsChanged;
                                nsa.Name = nsa.BookmarkFromAccount.Name;
                                SubAccounts.TryAdd(nsa.Address, nsa);
                            }
                        }
                    }
                }
                return "OK";
            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot deserialize the sub accounts.");
            }
            return string.Empty;
        }

        private void Nsa_NFTsChanged(object sender, string e)
        {
            NFTsChanged?.Invoke(sender, (sender as NeblioSubAccount).Address);
        }

        private void Nsa_NewMintingProcessInfo(object sender, string e)
        {
            NewMintingProcessInfo?.Invoke(sender, e);
        }

        /// <summary>
        /// Add new Sub Account
        /// </summary>
        /// <param name="address"></param>
        /// <param name="sendNeblioToAccount">Set This true if you want to load some Neblio to this address after it is created.</param>
        /// <param name="neblioAmountToSend">Amount of neblio for initial load of the address, 0.05 is default = 250 tx</param>
        /// <returns>true and string with serialized tabs list as json string</returns>
        public async Task<(bool, string)> AddSubAccount(string name,
                                                        bool sendNeblioToAccount = false,
                                                        double neblioAmountToSend = 0.05,
                                                        bool sendTokenToAccount = false,
                                                        double tokenAmountToSend = 10,
                                                        string tokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8")
        {
            if (!SubAccounts.Values.Any(a => a.Name == name))
            {
                var nsa = new NeblioSubAccount();
                var r = await nsa.CreateAddress(Secret, name);
                if (!r.Item1)
                {
                    await InvokeErrorEvent("Cannot create SubAccount Address." + r.Item2, "SubAccount Address Error");
                    return (false, "Cannot create SubAccount Address." + r.Item2);
                }
                await AddBookmark(name, nsa.Address, "SubAccount");
                var bkm = await IsInTheBookmarks(nsa.Address);
                nsa.LoadBookmark(bkm.Item2);
                nsa.IsAutoRefreshActive = true;
                nsa.NewEventInfo += Nsa_NewEventInfo;
                await nsa.StartRefreshingData();
                nsa.NewMintingProcessInfo += Nsa_NewMintingProcessInfo;
                nsa.NFTsChanged += Nsa_NFTsChanged;
                nsa.Name = name;
                SubAccounts.TryAdd(nsa.Address, nsa);

                (bool, string) res = (false, string.Empty);
                if (sendNeblioToAccount && !sendTokenToAccount)
                {
                    res = await SendNeblioPayment(nsa.Address, neblioAmountToSend);
                }
                else if (sendNeblioToAccount && sendTokenToAccount)
                {
                    res = await SendAirdrop(nsa.Address, tokenId, tokenAmountToSend, neblioAmountToSend);
                }
                else if (!sendNeblioToAccount && sendTokenToAccount)
                {
                    var meta = new Dictionary<string, string>();
                    meta.Add("Data", "Init token load.");
                    res = await SendNeblioTokenPayment(tokenId, meta, nsa.Address, (int)tokenAmountToSend);
                }
            }
            else
            {
                await InvokeErrorEvent("Same Name Already Exists. Please select another name.", "Already Exists");
                return (false, "Name Already Exists.");
            }

            return (true, await SerializeSubAccounts());
        }

        /// <summary>
        /// Remove Sub Account by Neblio address if exists in the dictionary
        /// Please remember that this function will destroy account. Please do backup first.
        /// </summary>
        /// <param name="address">Neblio Address which tab should be removed</param>
        /// <returns>true and string with serialized subaccount account export dto list as json string</returns>
        public async Task<(bool, string)> RemoveSubAccount(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
            {
                sacc.NewEventInfo -= Nsa_NewEventInfo;
                sacc.NewMintingProcessInfo -= Nsa_NewMintingProcessInfo;
                sacc.NFTsChanged -= Nsa_NFTsChanged;
                SubAccounts.Remove(address);
            }

            return (true, await SerializeSubAccounts());
        }

        /// <summary>
        /// Get sub account address by name
        /// </summary>
        /// <param name="name">Neblio Sub Account Name</param>
        /// <returns>true and string with serialized subaccount account export dto list as json string</returns>
        public async Task<(bool, string)> GetSubAccountAddressByName(string name)
        {
            var acc = SubAccounts.Values.FirstOrDefault(a => a.Name == name);
            if (acc != null)
            {
                return (true, acc.Address);
            }

            return (false, string.Empty);
        }

        /// <summary>
        /// Get sub account name by address
        /// </summary>
        /// <param name="name">Neblio Sub Account Name</param>
        /// <returns>true and string with serialized subaccount account export dto list as json string</returns>
        public async Task<(bool, string)> GetSubAccountNameByAddress(string address)
        {
            var acc = SubAccounts.Values.FirstOrDefault(a => a.Address == address);
            if (acc != null)
            {
                return (true, acc.Name);
            }

            return (false, string.Empty);
        }

        public double GetSubAccounTotaltSpendableActualBalance(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
                return sacc.TotalSpendableBalance;
            else
                return 0;
        }
        public double GetSubAccounUnconfirmedActualBalance(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
                return sacc.TotalUnconfirmedBalance;
            else
                return 0;
        }

        /// <summary>
        /// Change Sub Account Name if exists in the dictionary
        /// Automatically is changed name in bookmarks too. Thats why function will return both serialized lists
        /// </summary>
        /// <param name="address">Neblio Address which tab should be renamed</param>
        /// <param name="newName">New Name</param>
        /// <returns>true and string with serialized subaccount account export dto list as json string nad bookmarks list</returns>
        public async Task<(bool, (string, string))> ChangeSubAccountName(string address, string newName)
        {
            if (!SubAccounts.Values.Any(a => a.Name == newName))
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    sacc.Name = newName;
                    await RemoveBookmark(address);
                    await AddBookmark(newName, address, "SubAccount");
                    Bookmarks.Find(b => b.Address == address).IsSubAccount = true;
                }
            }
            else
            {
                await InvokeErrorEvent("Same Name Already Exists. Please select another name.", "Already Exists");
                return (false, ("Name Already Exists.", string.Empty));
            }

            return (true, (await SerializeSubAccounts(), await SerializeBookmarks()));
        }

        /// <summary>
        /// Get Sub Account Keys for export
        /// </summary>
        /// <returns>true and dictionary with addresses and private keys</returns>
        public async Task<(bool, Dictionary<string, string>)> GetSubAccountKeys()
        {
            try
            {
                var accskeys = new Dictionary<string, string>();
                foreach (var sa in SubAccounts.Values)
                {
                    var key = await sa.AccountKey.GetEncryptedKey();
                    if (!string.IsNullOrEmpty(key))
                        accskeys.Add(sa.Address, key);
                }

                return (true, accskeys);
            }
            catch (Exception ex)
            {
                return (false, new Dictionary<string, string>());
            }
        }

        /// <summary>
        /// Send NFT From SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <param name="NFT">NFT on the SubAccount which should be send</param>
        /// <param name="sendToMainAccount">If this is set, function will rewrite receiver to main Account Address</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> SendNFTFromSubAccount(string address, string receiver, INFT NFT, bool sendToMainAccount = false)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    if (sendToMainAccount)
                        receiver = Address;
                    var res = await sacc.SendNFT(receiver, NFT, false, 0.0);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Change NFT on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be changed</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> ChangeNFTOnSubAccount(string address, INFT NFT)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.ChangeNFT(NFT);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Mint NFT on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be minted</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> MintNFTOnSubAccount(string address, INFT NFT, string receiver = "")
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.MintNFT(NFT, receiver);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Multi Mint of large amount of NFTs on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be minted</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> MultimintNFTLargeOnSubAccount(string address, INFT NFT, int coppies, string receiver = "")
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.MintMultiNFTLargeAmount(NFT, coppies, receiver);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Split Neblio Coin on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be minted</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> SplitNeblioOnSubAccount(string address, List<string> receivers, int lots, double amount)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.SplitNeblioCoin(receivers, lots, amount);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Split Neblio Tokens on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be minted</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> SplitNeblioTokensOnSubAccount(string address, string tokenId, IDictionary<string, string> metadata, List<string> receivers, int lots, int amount)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.SplitTokens(tokenId, metadata, receivers, lots, amount);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Get QR verification code of NFT on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="txid">NFT utxo on the SubAccount</param>
        /// <returns></returns>
        public async Task<(OwnershipVerificationCodeDto, byte[])> GetNFTVerifyQRCodeFromSubAccount(string address, string txid)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.GetNFTVerifyQRCode(txid);
                    return res;
                }
                else
                    return (new OwnershipVerificationCodeDto(), new byte[0]);
            }
            catch (Exception ex)
            {
                return (new OwnershipVerificationCodeDto(), new byte[0]);
            }
        }

        /// <summary>
        /// Use Ticket NFT on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be changed</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> UseTicketNFTOnSubAccount(string address, INFT NFT)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.UseNFTTicket(NFT);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Change Coruzant NFT on SubAccount or send it to another address
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be changed</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> SendCoruzantNFTOnSubAccount(string address, INFT NFT, string comment = "", bool commentWrite = false, string receiver = "")
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var res = await sacc.SendCoruzantNFT(receiver, NFT, commentWrite, comment);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Destroy NFTs on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <param name="NFT">NFT on the SubAccount which should be send</param>
        /// /// <param name="sendToMainAccount">If this is set, function will rewrite receiver to main Account Address</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> DestroyNFTOnSubAccount(string address, ICollection<INFT> NFTs, bool sendToMainAccount = false)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    var receiver = string.Empty;
                    if (sendToMainAccount)
                        receiver = Address;
                    var res = await sacc.DestroyNFTs(NFTs, receiver);
                    return res;
                }
                else
                    return (false, "SubAccount is not in the list.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Get NFTs on SubAccount
        /// </summary>
        /// <param name="address">Neblio Address of SubAccount</param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, ICollection<INFT>)> GetNFTsOnSubAccount(string address)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                    return (true, sacc.NFTs);
                else
                    return (false, new List<INFT>());
            }
            catch (Exception ex)
            {
                return (false, new List<INFT>());
            }
        }

        public async Task AllowSubAccountAutorefreshing(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
                sacc.IsAutoRefreshActive = true;
        }
        public async Task StopSubAccountAutorefreshing(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
                sacc.IsAutoRefreshActive = false;
        }

        /// <summary>
        /// Returns serialized subaccount account export dto list as json string
        /// </summary>
        /// <returns></returns>
        public async Task<string> SerializeSubAccounts()
        {
            var dtos = new List<AccountExportDto>();
            foreach (var a in SubAccounts)
            {
                var dto = await a.Value.BackupAddressToDto();
                if (dto.Item1)
                    dtos.Add(dto.Item2);
            }
            return JsonConvert.SerializeObject(dtos);
        }

        #endregion

        #region DogePaymentsHandling

        public async Task ConnectDogeAccount(string address)
        {
            ConnectedDogeAccountAddress = address;
            if (VEDLDataContext.DogeAccounts.TryGetValue(address, out var doge))
            {
                doge.NewDogeUtxos += Doge_NewDogeUtxos;
                await CheckDogePayments();
            }
        }

        public async Task DisconnectDogeAccount()
        {
            if (VEDLDataContext.DogeAccounts.TryGetValue(ConnectedDogeAccountAddress, out var doge))
            {
                doge.NewDogeUtxos -= Doge_NewDogeUtxos;
            }
            ConnectedDogeAccountAddress = string.Empty;
        }

        private void Doge_NewDogeUtxos(object sender, IEventInfo e)
        {
            CheckDogePayments();
        }

        private async Task CheckDogePayments()
        {
            if (VEDLDataContext.DogeAccounts.TryGetValue(ConnectedDogeAccountAddress, out var doge))
            {
                var utxos = new DogeAPI.Utxo[doge.Utxos.Count];
                lock (_lock)
                {
                    doge.Utxos.CopyTo(utxos);
                }
                foreach (var u in utxos)
                {
                    if (u != null)
                    {
                        if (u.TxId == LastCheckedDogePaymentUtxo)
                            break;
                        if (u.Confirmations > 2)
                        {
                            var info = await DogeTransactionHelpers.TransactionInfoAsync(u.TxId);
                            if (info != null && info.Transaction != null)
                            {
                                var msg = await DogeTransactionHelpers.ParseDogeMessage(info);
                                if (msg.Item1)
                                {
                                    var split = msg.Item2.Split('-');
                                    if (split != null && split.Length == 2 && !string.IsNullOrEmpty(split[1]) && split[1].Contains(":") && split[1].Length == 18)
                                    {
                                        var nft = NFTs.FirstOrDefault(n => n.ShortHash == split[1]);
                                        if (nft == null)
                                        {
                                            foreach (var s in SubAccounts.Values)
                                            {
                                                nft = s.NFTs.FirstOrDefault(n => n.ShortHash == split[1]);
                                                if (nft != null)
                                                    break;
                                            }
                                        }

                                        if (nft != null)
                                        {
                                            if (nft.DogePriceActive && nft.DogePrice == Convert.ToDouble(u.Value, CultureInfo.InvariantCulture))
                                            {
                                                var addver = await NeblioTransactionHelpers.ValidateNeblioAddress(split[0]);
                                                if (addver.Item1)
                                                {
                                                    var done = false;
                                                    (bool, string) res = (false, string.Empty);
                                                    (bool, string) dres = (false, string.Empty);
                                                    var attempts = 50;
                                                    while (!done)
                                                    {
                                                        try
                                                        {
                                                            res = await SendNFT(addver.Item2, nft, false, 0.0002);
                                                            done = res.Item1;
                                                            if (!res.Item1) await Task.Delay(5000);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            await Task.Delay(5000);
                                                        }
                                                        attempts--;
                                                        if (attempts < 0) break;
                                                    }
                                                    if (res.Item1 && done)
                                                    {
                                                        LastCheckedDogePaymentUtxo = u.TxId;
                                                        done = false;
                                                        attempts = 50;
                                                        while (!done)
                                                        {
                                                            try
                                                            {
                                                                dres = await doge.SendPayment(doge.Address,
                                                                                              Convert.ToDouble(u.Value, CultureInfo.InvariantCulture) - 1,
                                                                                              "NFT:" + res.Item2);
                                                                done = dres.Item1;
                                                                if (!dres.Item1) await Task.Delay(5000);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                await Task.Delay(5000);
                                                            }
                                                            attempts--;
                                                            if (attempts < 0) break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LastCheckedDogePaymentUtxo = u.TxId;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Reload actual token supplies based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadTokenSupply()
        {
            TokensSupplies = await NeblioTransactionHelpers.CheckTokensSupplies(Address, AddressInfoUtxos);
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
        /// Reload address Utxos list. It will sort descending the utxos based on the utxos time stamps.
        /// </summary>
        /// <returns></returns>
        public async Task ReloadUtxos()
        {
            var ux = await NeblioTransactionHelpers.GetAddressUtxosObjects(Address);
            var ouxox = ux.OrderBy(u => u.Blocktime).Reverse().ToList();

            if (ouxox.Count > 0)
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
            }
            AddressInfoUtxos = new GetAddressInfoResponse()
            {
                Utxos = Utxos
            };
        }
        /// <summary>
        /// This function will load actual address info an adress utxos. It is used mainly for loading list of all transactions.
        /// </summary>
        /// <returns></returns>
        public async Task ReloadAccountInfo()
        {
            AddressInfo = await NeblioTransactionHelpers.AddressInfoAsync(Address);
            AddressInfoUtxos = await NeblioTransactionHelpers.AddressInfoUtxosAsync(Address);

            if (AddressInfo != null)
            {
                TotalBalance = (double)AddressInfo.Balance;
                TotalUnconfirmedBalance = (double)AddressInfo.UnconfirmedBalance;
                AddressInfo.Transactions = AddressInfo.Transactions.Reverse().ToList();
            }
            else
                AddressInfo = new GetAddressResponse();

            if (TotalBalance > 1)
                EnoughBalanceToBuySourceTokens = true;
        }

        /// <summary>
        /// This function will reload changes in the NFTs list based on provided list of already loaded utxos.
        /// </summary>
        /// <returns></returns>
        public async Task ReLoadNFTs(bool fireProfileEvent = false)
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
                    var lastnft = NFTs.FirstOrDefault();
                    var lastcount = NFTs.Count;
                    NFTs = await NFTHelpers.LoadAddressNFTs(Address, Utxos.ToList(), NFTs.ToList(), fireProfileEvent);
                    if (lastnft != null)
                    {
                        if (NFTs.Count != lastcount)
                            NFTsChanged?.Invoke(this, "Changed");
                        var nft = NFTs.FirstOrDefault();
                        //Console.WriteLine("Last time: " + lastnft.Time.ToString());
                        //Console.WriteLine("Newest time: " + nft.Time.ToString());
                        if (nft != null)
                            if (nft.Time != lastnft.Time)
                                NFTsChanged?.Invoke(this, "Changed");
                    }
                    else if (lastnft == null && NFTs.Count > 0)
                    {
                        NFTsChanged?.Invoke(this, "Changed");
                    }

                    CoruzantNFTs = await CoruzantNFTHelpers.GetCoruzantNFTs(NFTs);
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
            }
        }

        private void NFTHelpers_ProfileNFTFound(object sender, INFT e)
        {
            Profile = e as ProfileNFT;
            ProfileUpdated?.Invoke(this, e);
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
                ReceivedPayments.Clear();
                var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
                foreach (var p in pnfts)
                    ReceivedPayments.TryAdd(p.NFTOriginTxId, p);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot refresh address received payments. " + ex.Message);
            }
        }

        /// <summary>
        /// This function will check payments and try to find them complementary NFT which was sold. 
        /// If there is price mathc and enough of confirmations it will try to send NFT to new owner.
        /// </summary>
        /// <returns></returns>
        public async Task CheckPayments()
        {
            try
            {
                var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
                if (pnfts.Count > 0)
                {
                    foreach (var p in pnfts)
                    {
                        var pn = NFTs.Where(n => n.Utxo == ((PaymentNFT)p).NFTUtxoTxId).FirstOrDefault();
                        var prc = p.Price;

                        var prcn = pn.Price;
                        if (pn != null && pn.Price > 0 && p.Price >= pn.Price)
                        {
                            try
                            {
                                var res = await CheckSpendableNeblio(0.001);
                                if (res.Item2 != null)
                                {
                                    var rtxid = await NFTHelpers.SendOrderedNFT(Address, AccountKey, (PaymentNFT)p, pn, res.Item2);
                                    Console.WriteLine(rtxid);
                                    await Task.Delay(500);
                                    await ReLoadNFTs();
                                }
                            }
                            catch (Exception ex)
                            {
                                //await InvokeErrorDuringSendEvent($"Cannot send ordered NFT. Payment TxId: {p.Utxo}, NFT TxId: {pn.Utxo}, error message: {ex.Message}", "Cannot send ordered NFT");
                                Console.WriteLine("Cannot send ordered NFT, payment txid: " + p.Utxo + " - " + ex.Message);
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
        /// <param name="utxo"></param>
        /// <returns></returns>
        public async Task<(bool, string)> ValidateNFTUtxo(string utxo, int index)
        {
            var u = await NeblioTransactionHelpers.ValidateOneTokenNFTUtxo(Address, NFTHelpers.TokenId, utxo, index);
            if (!u.Item1)
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
                var nutxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(Address, 0.0002, amount);
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
                var tutxos = await NeblioTransactionHelpers.FindUtxoForMintNFT(Address, id, amount);
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

        /// <summary>
        /// This function will send request for 100 VENFT tokens. It can be process by sending 1 NEBL to specific project address.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(bool, string)> OrderSourceTokens(double amount = 1)
        {
            return await SendNeblioPayment("NRJs13ULX5RPqCDfEofpwxGptg5ePB8Ypw", amount);
        }

        /// <summary>
        /// Send classic neblio payment
        /// </summary>
        /// <param name="receiver">Receiver Neblio Address</param>
        /// <param name="amount">Ammount in Neblio</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNeblioPayment(string receiver, double amount)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(amount);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable inputs");
                return (false, res.Item1);
            }

            // fill input data for sending tx
            var dto = new SendTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = amount,
                SenderAddress = Address,
                ReceiverAddress = receiver
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
        /// Split neblio coin to smaller coins
        /// </summary>
        /// <param name="receiver">Receiver Neblio Address</param>
        /// <param name="splittedAmount">Ammount of new splitted coin in Neblio</param>
        /// <param name="count">Count of new splited couns</param>
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

            var res = await CheckSpendableNeblio(totalAmount + 0.0002);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable inputs");
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, amount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable token inputs");
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, totalAmount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable token inputs");
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

        /// <summary>
        /// Mint new NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(NFT.TokenId, 3);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable Token inputs. You need 3 tokens as minimum input.");
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
        /// <param name="tokenId"></param>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <param name="coppies">Number of coppies. 1 coppy means 2 final NFTs</param>
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(NFT.TokenId, 2 + coppies);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable Token inputs");
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
                //await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, (null, null));
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, 3 + coppies);
            if (tres.Item2 == null || tres.Item2.Count == 0)
            {
                //await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable Token inputs. You need 3 tokens as minimum input.");
                return (false, (null, null));
            }
            return (true, (res.Item2, tres.Item2));

        }

        /// <summary>
        /// Mint new multi NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <param name="coppies">Number of coppies. 1 coppy means 2 final NFTs</param>
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
                (bool, string) res = (false, string.Empty);
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
        /// <param name="tokenId">Token Id hash</param>
        /// <param name="nfts">List of NFTs</param>
        /// <returns></returns>
        public async Task<(bool, string)> DestroyNFTs(ICollection<INFT> nfts, string receiver = "")
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                // send tx
                var rtxid = await NFTHelpers.DestroyNFTs(Address, AccountKey, nfts, res.Item2, receiver);
                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFTs Destroyed.");
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
        /// This function will change profile NFT. It need as input previous loaded Profile NFT.
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task<(bool, string)> ChangeProfileNFT(INFT NFT)
        {
            var nft = await NFTFactory.CloneNFT(NFT);

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            if (string.IsNullOrEmpty(NFT.Utxo))
            {
                await InvokeErrorDuringSendEvent("Cannot change profile without providen Utxo TxId.", "Cannot change the profile.");
                return (false, "Cannot change Profile without provided Utxo TxId.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.ChangeNFT(Address, AccountKey, nft, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "Profile Changed");
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.ChangeNFT(Address, AccountKey, nft, res.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Post Changed");
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
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SendMessageNFT(string name, string message, string receiver, string utxo = "", bool encrypt = true)
        {
            MessageNFT nft = new MessageNFT("");

            nft.Utxo = utxo;
            nft.Description = message;
            nft.Name = name;
            nft.Encrypt = encrypt;
            nft.TokenId = MessagingTokensId;

            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }

            var tres = await CheckSpendableNeblioTokens(MessagingTokensId, 3);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable Token inputs");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = await NFTHelpers.SendMessageNFT(Address, receiver, AccountKey, nft, res.Item2, tres.Item2);

                if (rtxid != null)
                {
                    await InvokeSendPaymentSuccessEvent(rtxid, "NFT Post Changed");
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
        /// Send input NFT to new owner, or use it just for price write and resend it to yourself with new metadata about the price.
        /// </summary>
        /// <param name="receiver">If the pricewrite is set, this is filled with own address</param>
        /// <param name="NFT"></param>
        /// <param name="priceWrite">Set this if you need to just write the price to the NFT</param>
        /// <param name="price">Price must be bigger than 0.0002 NEBL</param>
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
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
            var res = await CheckSpendableNeblio(0.001);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
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
        /// This function will send airdrop.
        /// </summary>
        /// <param name="receiver">Receiver - owner of the NFT</param>
        /// <param name="NFT">NFT what you want to buy</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendAirdrop(string receiver, string tokenId, double tokenAmount, double neblioAmount)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }

            var res = await CheckSpendableNeblio(neblioAmount);
            if (res.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
                return (false, res.Item1);
            }
            var tres = await CheckSpendableNeblioTokens(tokenId, (int)tokenAmount);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable token inputs");
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
                await InvokeErrorDuringSendEvent(res.Item1, "Not enought spendable Neblio inputs");
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

        /// <summary>
        /// Sign custom message with use of account Private Key
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SignMessage(string message)
        {
            if (IsLocked())
            {
                await InvokeAccountLockedEvent();
                return (false, "Account is locked.");
            }
            var key = await AccountKey.GetEncryptedKey();
            return await ECDSAProvider.SignMessage(message, key);
        }

        /// <summary>
        /// Verify message which was signed by some address.
        /// </summary>
        /// <param name="message">Input message</param>
        /// <param name="signature">Signature of this message created by owner of some address.</param>
        /// <param name="address">Neblio address which should sign the message and should be verified.</param>
        /// <returns></returns>
        public async Task<(bool, string)> VerifyMessage(string message, string signature, string address)
        {
            var ownerpubkey = await NFTHelpers.GetPubKeyFromProfileNFTTx(address);
            if (!ownerpubkey.Item1)
                return (false, "Owner did not activate the function. He must have filled the profile.");
            else
                return await ECDSAProvider.VerifyMessage(message, signature, ownerpubkey.Item2);
        }

        /// <summary>
        /// Encrypt message with use of ECDSA
        /// </summary>
        /// <param name="message">Input message</param>
        /// <returns></returns>
        public async Task<(bool, string)> EncryptMessage(string message)
        {
            return await ECDSAProvider.EncryptMessage(message, Secret.PubKey.ToString());
        }

        /// <summary>
        /// Decrypt message with use of ECDSA
        /// </summary>
        /// <param name="message">Input message</param>
        /// <returns></returns>
        public async Task<(bool, string)> DecryptMessage(string emessage)
        {
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
            var res = await OwnershipVerifier.GetQRCode(txid, Secret);
            if (res.Item1)
                return (res.Item2.Item1, res.Item2.Item2);
            else
                return (new OwnershipVerificationCodeDto(), new byte[0]);
        }
    }
}
