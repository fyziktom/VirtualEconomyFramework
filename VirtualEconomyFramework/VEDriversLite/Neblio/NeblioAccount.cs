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
using VEDriversLite.NFT.DevicesNFTs;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;
using VEDriversLite.WooCommerce;

namespace VEDriversLite
{
    /// <summary>
    /// Main Neblio Account Class
    /// </summary>
    public class NeblioAccount : NeblioAccountBase
    {
        /// <summary>
        /// Main constructor
        /// </summary>
        public NeblioAccount()
        {
            Profile = new ProfileNFT("");
            NFTs = new List<INFT>();
            Bookmarks = new List<Bookmark>();
            InitHandlers();
            NFTHelpers.InitNFTHelpers();
        }

        private static object _lock { get; set; } = new object();

        /// <summary>
        /// Check of the DogeAccount for NFT payments - obsolete. will be redesign in the issue 33
        /// </summary>
        public string LastCheckedDogePaymentUtxo { get; set; } = string.Empty;
        /// <summary>
        /// This will block the automatic start of the NFT IoT Devices. 
        /// It is recommended to use NFT IoT devices in the services instead of the web assembly
        /// </summary>
        public bool RunningAsVENFTBlazorApp { get; set; } = false;

        /// <summary>
        /// List of all active tabs for browsing or interacting with the address. All has possibility to load own list of NFTs.
        /// </summary>
        public List<ActiveTab> Tabs { get; set; } = new List<ActiveTab>();
        /// <summary>
        /// Tabs with partners for messaging. It loads their NFT Messages related to you and mix them with yours related to the address in MessageTab
        /// </summary>
        public List<MessageTab> MessageTabs { get; set; } = new List<MessageTab>();
        /// <summary>
        /// Neblio Sub Accounts. Each can work with own set of NFTs. It is real blockchain address with own Private Key
        /// </summary>
        public Dictionary<string, NeblioSubAccount> SubAccounts { get; set; } = new Dictionary<string, NeblioSubAccount>();

        /// <summary>
        /// List of all saved bookmarks. This is just realtime carrier. It need some serialization/deserialization.
        /// </summary>
        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        /// <summary>
        /// This event is called whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;

        /// <summary>
        /// This event is called whenever some progress during multimint happens
        /// </summary>
        public event EventHandler<string> NewMintingProcessInfo;

        /// <summary>
        /// This event is called whenever the list of NFTs on SubAccount is changed
        /// </summary>
        public event EventHandler<string> SubAccountNFTsChanged;
        /// <summary>
        /// This event is called whenever the list of NFTs is changed
        /// </summary>
        public event EventHandler<string> TabNFTsChanged;
        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        public event EventHandler<string> FirsLoadingStatus;
        /// <summary>
        /// This event is called when first loading of the account is finished
        /// </summary>
        public event EventHandler<string> AccountFirsLoadFinished;

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
        /// This function will load the actual data and then run the task which periodically refresh this data.
        /// It doesnt have cancellation now!
        /// </summary>
        /// <param name="interval">Default interval is 3000 = 3 seconds</param>
        /// <returns></returns>
        public async Task<string> StartRefreshingData(int interval = 3000)
        {
            try
            {
                base.FirsLoadingStatus += NeblioAccount_FirsLoadingStatus;
                FirsLoadingStatus?.Invoke(this, "Loading of address data started.");

                AddressInfo = new GetAddressResponse();
                AddressInfo.Transactions = new List<string>();

                await ReloadUtxos();
                await Task.WhenAll(new Task[3] {
                                ReloadMintingSupply(),
                                ReloadTokenSupply(),
                                ExchangePriceService.InitPriceService(Cryptocurrencies.ExchangeRatesAPITypes.Coingecko, Address, Cryptocurrencies.CurrencyTypes.NEBL)
                });

                FirsLoadingStatus?.Invoke(this, "Utxos loaded");
                Refreshed?.Invoke(this, null);

                if (!WithoutNFTs)
                {
                    FirsLoadingStatus?.Invoke(this, "Loading NFTs started.");

                    try
                    {
                        await TxCashPreload();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot finish the preload." + ex.Message);
                    }

                    await ReLoadNFTs(true);

                    var tasks = new Task[5];
                    tasks[0] = ReloadCoruzantNFTs();
                    tasks[1] = ReloadHardwarioNFTs();
                    tasks[2] = ReloadCountOfNFTs();
                    tasks[3] = RefreshAddressReceivedPayments();
                    tasks[4] = RefreshAddressReceivedReceipts();
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
            base.FirsLoadingStatus -= NeblioAccount_FirsLoadingStatus;
            var minorRefresh = 2;
            var firstLoad = true;
            AccountFirsLoadFinished?.Invoke(Address, "OK");

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                IsRefreshingRunning = true;
                var tasks = new Task[5];

                while (true)
                {
                    try
                    {
                        if (!firstLoad)
                        {
                            await ReloadUtxos();

                            await Task.WhenAll(new Task[2] { 
                                ReloadMintingSupply(), 
                                ReloadTokenSupply() 
                            });
                        }

                        if (!WithoutNFTs)
                        {
                            if (!firstLoad)
                            {
                                await ReLoadNFTs(true);

                                tasks[0] = ReloadCoruzantNFTs();
                                tasks[1] = ReloadHardwarioNFTs();
                                tasks[2] = ReloadCountOfNFTs();
                                tasks[3] = RefreshAddressReceivedPayments();
                                tasks[4] = RefreshAddressReceivedReceipts();

                                await Task.WhenAll(tasks);
                            }

                            minorRefresh--;
                            if (minorRefresh < 0)
                            {
                                try
                                {
                                    if (!RunningAsVENFTBlazorApp)
                                    {
                                        try
                                        {
                                            await InitAllAutoIoTDeviceNFT();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Cannot init the IoT Devices. " + ex.Message);
                                        }
                                        await Task.Delay(500);
                                    }
                                    await CheckPayments();

                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine($"Cannot process the payments. {ex.Message}");
                                }
                                minorRefresh = 5;
                            }
                        }
                        Refreshed?.Invoke(Address, null);
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
                    {
                        await Task.Delay(interval);
                    }
                }
            });
            IsRefreshingRunning = false;
            return await Task.FromResult("RUNNING");
        }

        private void NeblioAccount_FirsLoadingStatus(object sender, string e)
        {
            FirsLoadingStatus?.Invoke(sender, e);
        }

        /// <summary>
        /// This function will create new account - Neblio address and its Private key.
        /// </summary>
        /// <param name="password">Input password, which will encrypt the Private key</param>
        /// <param name="saveToFile">if you want to save it to the file (dont work in the WASM) set this. It will save to root exe path as key.txt</param>
        /// <param name="filename">default filename is key.txt you can change it, but remember to load same name when loading the account.</param>
        /// <returns></returns>
        public async Task<bool> CreateNewAccount(string password, bool saveToFile = false, string filename = "key.txt")
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

                //SignMessage("init");

                if (saveToFile)
                {
                    // save to file
                    var kdto = new KeyDto()
                    {
                        Address = Address,
                        Key = AccountKey.GetEncryptedKey(returnEncrypted: true)
                    };
                    FileHelpers.WriteTextToFile(filename, JsonConvert.SerializeObject(kdto));
                }

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
        /// <param name="filename">filename with stored key.</param>
        /// <param name="withoutNFTs">choose if you want to skip NFTs during loading the account. 
        /// Great when you want just do simple payment. 
        /// You can then swithc off WithoutNFTs property and account will load them in next refresh.</param>
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password, string filename = "key.txt", bool withoutNFTs = false)
        {
            if (FileHelpers.IsFileExists(filename))
            {
                try
                {
                    var k = FileHelpers.ReadTextFromFile(filename);
                    var kdto = JsonConvert.DeserializeObject<KeyDto>(k);

                    Address = kdto.Address;

                    LoadAccountKey(password, kdto.Key);

                    SignMessage("init");

                    WithoutNFTs = withoutNFTs;
                    if (!IsRefreshingRunning)
                        await StartRefreshingData();

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address! " + ex.Message);
                }
            }
            else
            {
                CreateNewAccount(password);
            }

            return false;
        }

        /// <summary>
        /// Load account just for observation
        /// You cannot sign tx when you load address this way
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <param name="address">Account Address</param>
        /// <param name="withoutNFTs">choose if you want to skip NFTs during loading the account. 
        /// Great when you want just do simple payment. 
        /// You can then swithc off WithoutNFTs property and account will load them in next refresh.</param>
        /// <returns></returns>
        public async Task<bool> LoadAccountWithDummyKey(string password, string address, bool withoutNFTs = false)
        {
            try
            {
                Key k = new Key();
                BitcoinSecret privateKeyFromNetwork = k.GetBitcoinSecret(NeblioTransactionHelpers.Network);
                if (string.IsNullOrEmpty(password))
                    AccountKey = new EncryptionKey(privateKeyFromNetwork.ToString(), fromDb: true);
                else
                    AccountKey = new EncryptionKey(privateKeyFromNetwork.ToString(), password, fromDb: false);
                
                Secret = privateKeyFromNetwork;
                Address = address;//Secret.GetAddress(ScriptPubKeyType.Legacy).ToString();

                SignMessage("init");

                WithoutNFTs = withoutNFTs;
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
        /// <param name="fromString">BackupDataDto serialized object with key and address</param>
        /// <param name="filename">load from BackupDataDto from file. backup.json as default.</param>
        /// <param name="withoutNFTs">choose if you want to skip NFTs during loading the account. 
        /// Great when you want just do simple payment. 
        /// You can then swithc off WithoutNFTs property and account will load them in next refresh.</param>
        /// <returns></returns>
        public async Task<bool> LoadAccountFromVENFTBackup(string password, string fromString = "", string filename = "backup.json", bool withoutNFTs = false)
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

                    LoadAccountKey(password, bdto.Key);

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

                    WithoutNFTs = withoutNFTs;
                    SignMessage("init");

                    if (!IsRefreshingRunning)
                        await StartRefreshingData();

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!" + ex.Message);
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
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <param name="encryptedKey">Private key encrypted with AES (you must provide pass in this case) or not encrypted (you do not need password)</param>
        /// <param name="address">Neblio Address related to the private key (if empty it will be calculated from the private key).</param>
        /// <param name="withoutNFTs">choose if you want to skip NFTs during loading the account. 
        /// Great when you want just do simple payment. 
        /// You can then swithc off WithoutNFTs property and account will load them in next refresh.</param>
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password, string encryptedKey, string address = "", bool withoutNFTs = false)
        {
            try
            {
                var res = LoadAccountKey(password, encryptedKey);
                if (res)
                {
                    //SignMessage("init");
                    if (string.IsNullOrEmpty(address))
                    {
                        var add = NeblioTransactionHelpers.GetAddressFromPrivateKey(Secret.ToString());
                        if (!string.IsNullOrEmpty(add)) Address = add;
                    }
                    else
                    {
                        Address = address;
                    }
                }

                WithoutNFTs = withoutNFTs;
                if (!IsRefreshingRunning)
                    await StartRefreshingData();

                return true;
            }
            catch (Exception ex)
            {
                //await InvokeErrorEvent(ex.Message, "Cannot Load Account");
                Console.WriteLine("Cannot load the account! " + ex.Message);
                //throw new Exception("Cannot deserialize key from file. Please check file key.txt or delete it for create new address!");
            }

            return false;
        }

        /// <summary>
        /// Serialize the NFTCache dictionary
        /// </summary>
        /// <returns>Serialized VEDLDataContext.NFTCache</returns>
        public string CacheNFTs()
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
            catch(Exception ex)
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
        public bool LoadCacheNFTsFromString(string cacheString)
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
            catch(Exception ex)
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
        public bool LoadCacheNFTsFromString(IDictionary<string, NFTCacheDto> nfts)
        {
            try
            {
                if (nfts != null)
                {
                    lock (_lock)
                    {
                        VEDLDataContext.NFTCache.Clear();
                        foreach(var n in nfts)
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
        public string SerializeBookmarks()
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

        private void T_NFTAddedToPayments(object sender, (string, int) e)
        {
            FireNFTAddedToPayments(sender as string, e);
        }

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
                {
                    lock (_lock)
                    {
                        Tabs = tbs;
                    }
                }
                var firstAdd = string.Empty;
                if (Tabs.Count > 0)
                {
                    var first = true;
                    foreach (var t in Tabs)
                    {
                        var bkm = await IsInTheBookmarks(t.Address);
                        t.LoadBookmark(bkm.Item2);
                        if (first)
                        {
                            await t.StartRefreshing();
                            t.Selected = true;
                            first = false;
                            firstAdd = t.Address;
                        }
                        t.Selected = false;
                        t.NFTsChanged += T_NFTsChanged;
                        t.NFTAddedToPayments += T_NFTAddedToPayments;
                    }
                }
                return firstAdd;
            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot deserialize the tabs.");
            }
            return string.Empty;
        }

        private void T_NFTsChanged(object sender, string e)
        {
            TabNFTsChanged?.Invoke(sender, e);
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
                tab.NFTsChanged += T_NFTsChanged;
                tab.NFTAddedToPayments += T_NFTAddedToPayments;

                foreach (var t in Tabs)
                    t.StopRefreshing();

                await tab.StartRefreshing();
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
            {
                tab.StopRefreshing();
                tab.NFTsChanged -= T_NFTsChanged;
                tab.NFTAddedToPayments -= T_NFTAddedToPayments;

                Tabs.Remove(tab);
            }
            else
            {
                await InvokeErrorEvent("Tab Not Found.", "Not Found");
                return (false, string.Empty);
            }

            if (Tabs.Count > 0)
            {
                foreach (var t in Tabs)
                    t.StopRefreshing();
                await Tabs.FirstOrDefault().StartRefreshing();
            }
            return (true, JsonConvert.SerializeObject(Tabs));
        }
        /// <summary>
        /// Select active tab based on Address. It will deselect all other tabs
        /// This will start the refreshing for the selected tab if is not running yet and stop the others tabs refreshing.
        /// </summary>
        /// <param name="address">Address of tab to select</param>
        /// <returns></returns>
        public async Task SelectTab(string address)
        {
            Tabs.ForEach(async (t) => {
                if (t.Address == address)
                {
                    t.Selected = true;
                    if (!t.IsRefreshingRunning) await t.StartRefreshing();
                }
                else
                {
                    t.Selected = false;
                    t.StopRefreshing();
                }
            });
        }

        /// <summary>
        /// Return serialized list of ActiveTabs as Json stirng
        /// </summary>
        /// <returns></returns>
        public string SerializeTabs()
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
        /// <summary>
        /// Select Message tab. It will Reload the tab NFTs if the count of the NFTs is 0.
        /// </summary>
        /// <param name="address">Address of Message Tab to select</param>
        /// <returns></returns>
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
        public string SerializeMessageTabs()
        {
            return JsonConvert.SerializeObject(MessageTabs);
        }

        #endregion

        #region SubAccounts

        /*
        private void Nsa_NewEventInfo(object sender, IEventInfo e)
        {
            NewEventInfo?.Invoke(sender, e);
        }
        */
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
                                
                                nsa.NewMintingProcessInfo += Nsa_NewMintingProcessInfo;
                                nsa.NewEventInfo += Nsa_NewEventInfo;
                                nsa.NFTsChanged += Nsa_NFTsChanged;
                                nsa.NFTAddedToPayments += Nsa_NFTAddedToPayments;
                                nsa.FirsLoadingStatus += Nsa_NFTLoadingStateChangedHandler;
                                await nsa.StartRefreshingData();
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

        private void Nsa_NewEventInfo(object sender, IEventInfo e)
        {
            FireInfoEvent(sender, e);
        }

        private void Nsa_NFTsChanged(object sender, string e)
        {
           SubAccountNFTsChanged?.Invoke(sender, (sender as NeblioSubAccount).Address);
        }

        private void Nsa_NFTAddedToPayments(object sender, (string,int) e)
        {
           FireNFTAddedToPayments(sender as string, e);
        }

        private void Nsa_NewMintingProcessInfo(object sender, string e)
        {
            NewMintingProcessInfo?.Invoke(sender, e);
        }

        private void Nsa_NFTLoadingStateChangedHandler(object sender, string e)
        {
            FirsLoadingStatus?.Invoke(this, e);
        }

        /// <summary>
        /// Add new Sub Account
        /// </summary>
        /// <param name="name">Name of new SubAccount</param>
        /// <param name="sendNeblioToAccount">Set This true if you want to load some Neblio to this address after it is created.</param>
        /// <param name="neblioAmountToSend">Amount of neblio for initial load of the address, 0.05 is default = 250 tx</param>
        /// <param name="sendTokenToAccount">Initial amount of tokens to send to the new SubAccount</param>
        /// <param name="tokenAmountToSend">Initial amount of Neblio to send to the new SubAccount</param>
        /// <param name="tokenId">Token Id which should be send to the new SubAccount</param>
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
                nsa.NewMintingProcessInfo += Nsa_NewMintingProcessInfo;
                nsa.NFTsChanged += Nsa_NFTsChanged;
                nsa.FirsLoadingStatus += Nsa_NFTLoadingStateChangedHandler;
                nsa.NFTAddedToPayments += Nsa_NFTAddedToPayments;
                await nsa.StartRefreshingData();
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

            return (true, SerializeSubAccounts());
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
                sacc.NewMintingProcessInfo -= Nsa_NewMintingProcessInfo;
                sacc.NFTsChanged -= Nsa_NFTsChanged;
                sacc.NewEventInfo -= Nsa_NewEventInfo;
                sacc.FirsLoadingStatus -= Nsa_NFTLoadingStateChangedHandler;
                sacc.NFTAddedToPayments -= Nsa_NFTAddedToPayments;
                SubAccounts.Remove(address);
            }

            return (true, SerializeSubAccounts());
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
        /// <param name="address">Neblio Sub Account Name</param>
        /// <returns>true and string with serialized subaccount account export dto list as json string</returns>
        public (bool, string) GetSubAccountNameByAddress(string address)
        {
            var acc = SubAccounts.Values.FirstOrDefault(a => a.Address == address);
            if (acc != null)
            {
                return (true, acc.Name);
            }

            return (false, string.Empty);
        }

        /// <summary>
        /// Get Total spendable balance
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public double GetSubAccounTotaltSpendableActualBalance(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
                return sacc.TotalSpendableBalance;
            else
                return 0;
        }
        /// <summary>
        /// Get Total unconfirmed balance
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
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

            return (true, (SerializeSubAccounts(), SerializeBookmarks()));
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
                    var key = sa.AccountKey.GetEncryptedKey();
                    if (!string.IsNullOrEmpty(key))
                        accskeys.Add(sa.Address, key);
                }

                return (true, accskeys);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Canont get sub account keys. " + ex.Message);
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
        /// <param name="withPrice"></param>
        /// <param name="price"></param>
        /// <param name="withDogePrice"></param>
        /// <param name="dogeprice"></param>
        /// <returns>true and string with new TxId</returns>
        public async Task<(bool, string)> SendNFTFromSubAccount(string address, string receiver, INFT NFT, bool sendToMainAccount = false, bool withPrice = false, double price = 0.0, bool withDogePrice = false, double dogeprice = 0.0)
        {
            try
            {
                if (SubAccounts.TryGetValue(address, out var sacc))
                {
                    if (sendToMainAccount)
                        receiver = Address;
                    var res = await sacc.SendNFT(receiver, NFT, withPrice, price, withDogePrice, dogeprice);
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
        /// <param name="receiver">Receiver of the NFT</param>
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
        /// <param name="receiver">Receiver of the NFT</param>
        /// <param name="coppies"></param>
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
        /// <param name="receivers"></param>
        /// <param name="lots"></param>
        /// <param name="amount"></param>
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
        /// <param name="tokenId">Id of token to split</param>
        /// <param name="metadata">metadata</param>
        /// <param name="receivers">list of the receivers of the transaction.</param>
        /// <param name="lots">Number of lots on the Output of tx.</param>
        /// <param name="amount">Amount of the tokens in one lot.</param>
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
                Console.WriteLine("Cannot get NFT Verify code. " + ex.Message);
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
        /// <param name="comment"></param>
        /// <param name="commentWrite"></param>
        /// <param name="receiver"></param>
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
        /// <param name="NFTs">NFTs on the SubAccount which should be send</param>
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
        public (bool, ICollection<INFT>) GetNFTsOnSubAccount(string address)
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
                Console.WriteLine("Cannot get NFTs from sub account. " + ex.Message);
                return (false, new List<INFT>());
            }
        }

        /// <summary>
        /// Allow autorefresh for specific SubAccount
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task AllowSubAccountAutorefreshing(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
                sacc.IsAutoRefreshActive = true;
        }
        /// <summary>
        /// Stop autorefresh for specific SubAccount
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task StopSubAccountAutorefreshing(string address)
        {
            if (SubAccounts.TryGetValue(address, out var sacc))
                sacc.IsAutoRefreshActive = false;
        }

        /// <summary>
        /// Returns serialized subaccount account export dto list as json string
        /// </summary>
        /// <returns></returns>
        public string SerializeSubAccounts()
        {
            var dtos = new List<AccountExportDto>();
            foreach (var a in SubAccounts)
            {
                var dto = a.Value.BackupAddressToDto();
                if (dto.Item1)
                    dtos.Add(dto.Item2);
            }
            return JsonConvert.SerializeObject(dtos);
        }

        #endregion

        #region DogePaymentsHandling
        // will be redesing

        /// <summary>
        /// connect Doge Account related to this account
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task ConnectDogeAccount(string address)
        {
            ConnectedDogeAccountAddress = address;
            if (VEDLDataContext.DogeAccounts.TryGetValue(address, out var doge))
            {
                doge.NewDogeUtxos += Doge_NewDogeUtxos;
                await CheckDogePayments();
            }
        }
        /// <summary>
        /// Remove the connection of some Doge Account
        /// </summary>
        /// <returns></returns>
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
                                var msg = DogeTransactionHelpers.ParseDogeMessage(info);
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
                                                var addver = NeblioTransactionHelpers.ValidateNeblioAddress(split[0]);
                                                if (!string.IsNullOrEmpty(addver))
                                                {
                                                    var done = false;
                                                    (bool, string) res = (false, string.Empty);
                                                    (bool, string) dres = (false, string.Empty);
                                                    var attempts = 50;
                                                    while (!done)
                                                    {
                                                        try
                                                        {
                                                            res = await SendNFT(addver, nft, false, 0.0002);
                                                            done = res.Item1;
                                                            if (!res.Item1) await Task.Delay(5000);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Console.WriteLine("Cannot send NFT. Waiting for confirmation? " + ex.Message);
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
                                                                Console.WriteLine("Cannot send the doge Payment. Probably waiting for confirmations." + ex.Message);
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
        /// This function will send request for 100 VENFT tokens. It can be process by sending 1 NEBL to specific project address.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(bool, string)> OrderSourceTokens(double amount = 1)
        {
            return await SendNeblioPayment("NRJs13ULX5RPqCDfEofpwxGptg5ePB8Ypw", amount);
        }

    }
}
