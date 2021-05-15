using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.Events;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.Security;

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
        }
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
        /// Total number of NFT on the address. It counts also Profile NFT, etc.
        /// </summary>
        public double AddressNFTCount { get; set; } = 0.0;
        /// <summary>
        /// List of actual address NFTs. Based on Utxos list
        /// </summary>
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// List of all active tabs for browsing or interacting with the address. All has possibility to load own list of NFTs.
        /// </summary>
        public List<ActiveTab> Tabs { get; set; } = new List<ActiveTab>();
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
        
        /// <summary>
        /// This event is called whenever info about the address is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;
        /// <summary>
        /// This event is called whenever some important thing happen. You can obtain success, error and info messages.
        /// </summary>
        public event EventHandler<IEventInfo> NewEventInfo;

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
                //await ReloadAccountInfo();
                await ReloadUtxos();
                await ReloadMintingSupply();
                await ReloadCountOfNFTs();
                await ReloadTokenSupply();
            }
            catch (Exception ex)
            {
                // todo
            }

            var minorRefresh = 5;

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                try
                {
                    await ReLoadNFTs();
                    Profile = await NFTHelpers.FindProfileNFT(NFTs);
                    await CheckPayments();
                    await RefreshAddressReceivedPayments();
                    
                }
                catch(Exception ex)
                {
                    // todo
                }
                var lastNFTcount = AddressNFTCount;
                while (true)
                {
                    try
                    {
                        //await ReloadAccountInfo();
                        await ReloadUtxos();
                        await ReloadMintingSupply();
                        await ReloadCountOfNFTs();
                        await ReloadTokenSupply();

                        //if (lastNFTcount != AddressNFTCount)
                        await ReLoadNFTs();

                        minorRefresh--;
                        if (minorRefresh < 0)
                        {
                            Profile = await NFTHelpers.FindProfileNFT(NFTs);
                            await CheckPayments();
                            await RefreshAddressReceivedPayments();
                            minorRefresh = 10;
                        }

                        lastNFTcount = AddressNFTCount;

                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        //await InvokeErrorEvent(ex.Message, "Unknown Error During Refreshing Data");
                    }

                    await Task.Delay(interval);
                }

            });

            return await Task.FromResult("RUNNING");
        }

        /// <summary>
        /// This function will create new account - Neblio address and its Private key.
        /// </summary>
        /// <param name="password">Input password, which will encrypt the Private key</param>
        /// <param name="saveToFile">if you want to save it to the file (dont work in the WASM) set this. It will save to root exe path as key.txt</param>
        /// <returns></returns>
        public async Task<bool> CreateNewAccount(string password, bool saveToFile = false)
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
                       FileHelpers.WriteTextToFile("key.txt", JsonConvert.SerializeObject(kdto));
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
        /// Load account from "key.txt" file placed in the root exe directory. Doesnt work in WABS
        /// </summary>
        /// <param name="password">Passwotd to decrypt the loaded private key</param>
        /// <returns></returns>
        public async Task<bool> LoadAccount(string password)
        {
            if (FileHelpers.IsFileExists("key.txt"))
            {
                try
                {
                    var k = FileHelpers.ReadTextFromFile("key.txt");
                    var kdto = JsonConvert.DeserializeObject<KeyDto>(k);

                    AccountKey = new EncryptionKey(kdto.Key, fromDb: true);
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;
                    Address = kdto.Address;

                    Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), NeblioTransactionHelpers.Network);
                    SignMessage("init");
                   
                    await StartRefreshingData();
                }
                catch(Exception ex)
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
                    AccountKey = new EncryptionKey(encryptedKey, fromDb: true);
                    await AccountKey.LoadPassword(password);
                    AccountKey.IsEncrypted = true;
                    Secret = new BitcoinSecret(await AccountKey.GetEncryptedKey(), NeblioTransactionHelpers.Network);
                    SignMessage("init");
                    Address = address;
                });

                await StartRefreshingData();

            }
            catch (Exception ex)
            {
                await InvokeErrorEvent(ex.Message, "Cannot Load Account");
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
            catch(Exception ex)
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
        public async Task<(bool,string)> AddBookmark(string name, string address, string note)
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
        public async Task<(bool,string)> RemoveBookmark(string address)
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

            return (true,JsonConvert.SerializeObject(Bookmarks));
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

        /// <summary>
        /// Reload actual token supplies based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadTokenSupply()
        {
            TokensSupplies = await NeblioTransactionHelpers.CheckTokensSupplies(Address, AddressInfoUtxos);
        }

        /// <summary>
        /// Reload actual count of the NFTs based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadCountOfNFTs()
        {
            var nftsu = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address, AddressInfoUtxos);
            if (nftsu != null)
                AddressNFTCount = nftsu.Count;
        }

        /// <summary>
        /// Reload actual VENFT minting supply based on already loaded list of address utxos
        /// </summary>
        /// <returns></returns>
        public async Task ReloadMintingSupply()
        {
            var mintingSupply = await NeblioTransactionHelpers.GetActualMintingSupply(Address, AddressInfoUtxos);
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
            AddressInfoUtxos = new GetAddressInfoResponse()
            {
                Utxos = ouxox
            };

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
        public async Task ReLoadNFTs()
        {
            if (!string.IsNullOrEmpty(Address))
                NFTs = await NFTHelpers.LoadAddressNFTs(Address, Utxos.ToList(), NFTs.ToList());
                //NFTs = await NFTHelpers.LoadAddressNFTs(Address, Utxos.Values.ToList(), NFTs.ToList());
        }

        /// <summary>
        /// This function will search NFT Payments in the NFTs list and load them into ReceivedPayments list. 
        /// This list is cleared at the start of this function
        /// </summary>
        /// <returns></returns>
        public async Task RefreshAddressReceivedPayments()
        {
            ReceivedPayments.Clear();
            var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
            if (pnfts.Count > 0)
            {
                foreach (var p in pnfts)
                {
                    ReceivedPayments.TryAdd(p.NFTOriginTxId, p);
                }
            }
        }

        /// <summary>
        /// This function will check payments and try to find them complementary NFT which was sold. 
        /// If there is price mathc and enough of confirmations it will try to send NFT to new owner.
        /// </summary>
        /// <returns></returns>
        public async Task CheckPayments()
        {
            var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
            if (pnfts.Count > 0)
            {
                foreach (var p in pnfts)
                {
                    var pn = NFTs.Where(n => n.Utxo == ((PaymentNFT)p).NFTUtxoTxId).FirstOrDefault();
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

        /// <summary>
        /// This function will check if the address has some spendable neblio for transaction.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<(bool,double)> HasSomeSpendableNeblio(double amount = 0.0002)
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
        public async Task<(bool, string)> ValidateNFTUtxo(string utxo)
        {
            var u = await NeblioTransactionHelpers.ValidateOneTokenNFTUtxo(Address, NFTHelpers.TokenId, utxo);
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
            catch(Exception ex)
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
        /// Send classic token payment. It must match same requirements as minting. It cannot use 1 token inputs (NFTs).
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <param name="metadata">Custom metadata</param>
        /// <param name="receiver">Receiver Neblio address</param>
        /// <param name="amount">Amount of the tokens</param>
        /// <returns></returns>
        public async Task<(bool, string)> SendNeblioTokenPayment(string tokenId, IDictionary<string,string> metadata, string receiver, int amount)
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
        /// Mint new NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <returns></returns>
        public async Task<(bool, string)> MintNFT(string tokenId, INFT NFT)
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
            var tres = await CheckSpendableNeblioTokens(tokenId, 2);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable Token inputs");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = string.Empty;
                switch (NFT.Type)
                {
                    case NFTTypes.Image:
                        rtxid = await NFTHelpers.MintImageNFT(Address, AccountKey, nft, res.Item2, tres.Item2);
                        break;
                    case NFTTypes.Post:
                        rtxid = await NFTHelpers.MintPostNFT(Address, AccountKey, nft, res.Item2, tres.Item2);
                        break;
                    case NFTTypes.Music:
                        rtxid = await NFTHelpers.MintMusicNFT(Address, AccountKey, nft, res.Item2, tres.Item2);
                        break;
                    case NFTTypes.Profile:
                        rtxid = await NFTHelpers.MintProfileNFT(Address, AccountKey, nft, res.Item2, tres.Item2);
                        break;
                }
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

        /// <summary>
        /// Mint new multi NFT. It is automatic function which will decide what NFT to mint based on provided type in the NFT input.
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="NFT">Input carrier of NFT data. It must specify the type</param>
        /// <param name="coppies">Number of coppies. 1 coppy means 2 final NFTs</param>
        /// <returns></returns>
        public async Task<(bool, string)> MintMultiNFT(string tokenId, INFT NFT, int coppies)
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
            var tres = await CheckSpendableNeblioTokens(tokenId, 2 + coppies);
            if (tres.Item2 == null)
            {
                await InvokeErrorDuringSendEvent(tres.Item1, "Not enought spendable Token inputs");
                return (false, tres.Item1);
            }

            try
            {
                var rtxid = string.Empty;
                switch (NFT.Type)
                {
                    case NFTTypes.Image:
                        rtxid = await NFTHelpers.MintMultiImageNFT(Address, coppies, AccountKey, nft, res.Item2, tres.Item2);
                        break;
                    case NFTTypes.Post:
                        rtxid = await NFTHelpers.MintMultiPostNFT(Address, coppies, AccountKey, nft, res.Item2, tres.Item2);
                        break;
                    case NFTTypes.Music:
                        rtxid = await NFTHelpers.MintMultiMusicNFT(Address, coppies, AccountKey, nft, res.Item2, tres.Item2);
                        break;
                }
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
                var rtxid = await NFTHelpers.ChangeProfileNFT(Address, AccountKey, nft, res.Item2);

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
        /// Change Post NEFt. It requeires previous loadedPost NFT as input.
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public async Task<(bool, string)> ChangePostNFT(INFT NFT)
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
                var rtxid = await NFTHelpers.ChangePostNFT(Address, AccountKey, nft, res.Item2);

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
        public async Task<(bool, string)> SendNFT(string receiver, INFT NFT, bool priceWrite, double price)
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
                var rtxid = await NFTHelpers.SendNFT(Address, receiver, AccountKey, nft, priceWrite, res.Item2, price);

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
            return await ECDSAProvider.SignMessage(message, key); ;
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
