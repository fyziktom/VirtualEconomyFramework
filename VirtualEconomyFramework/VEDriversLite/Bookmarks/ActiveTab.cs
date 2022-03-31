using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;

namespace VEDriversLite.Bookmarks
{
    /// <summary>
    /// Active tab is able to load the Address and its NFTs
    /// </summary>
    public class ActiveTab
    {
        private static object _lock = new object();
        /// <summary>
        /// Basic constructor
        /// </summary>
        public ActiveTab() { }
        /// <summary>
        /// Basic constructor witch will load address and shortaddress
        /// </summary>
        /// <param name="address"></param>
        public ActiveTab(string address)
        {
            Address = address;
            ShortAddress = NeblioTransactionHelpers.ShortenAddress(address);
        }

        /// <summary>
        /// Info if the Tab is selected - this is set by Start or Stop functions internally
        /// </summary>
        [JsonIgnore]
        public bool Selected { get; set; } = false;
        /// <summary>
        /// Address loaded in this ActiveTab
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Shorten version of the address - just help for UI
        /// </summary>
        public string ShortAddress { get; set; } = string.Empty;
        /// <summary>
        /// Flag if the Address is in the bookmark - then it has Bookmark dto loaded
        /// </summary>
        public bool IsInBookmark { get; set; } = false;
        /// <summary>
        /// Tab loads just 40 NFTs. if it can load more, this is true
        /// </summary>
        public bool CanLoadMore { get; set; } = false;
        /// <summary>
        /// Indicate if the autorefresh is running
        /// </summary>
        [JsonIgnore]
        public bool IsRefreshingRunning { get; set; } = false;
        /// <summary>
        /// List of the loaded NFTs of the Address
        /// </summary>
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// List of the loaded Utxos of the Address
        /// </summary>
        [JsonIgnore]
        public ICollection<Utxos> UtxosList { get; set; } = new List<Utxos>();
        /// <summary>
        /// Loaded Bookmark data
        /// </summary>
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        /// <summary>
        /// Loaded list of the received payments NFTs of the address
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// Profile NFT of the Address
        /// </summary>
        [JsonIgnore]
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");
        /// <summary>
        /// This event is called whenever the list of NFTs is changed
        /// </summary>
        public event EventHandler<string> NFTsChanged;
        /// <summary>
        /// This event is called whenever profile nft is updated or found
        /// </summary>
        public event EventHandler<INFT> ProfileUpdated;
        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        public event EventHandler<string> FirsLoadingStatus;
        /// <summary>
        /// This event is fired whenever some NFT is in received payment too and it should be blocked for any further action.
        /// It provides Utxo and UtxoIndex as touple.
        /// </summary>
        public event EventHandler<(string, int)> NFTAddedToPayments;

        private bool withoutMsgs = true;
        private bool cachePreload = true;


        private System.Timers.Timer refreshTimer = new System.Timers.Timer();
        /// <summary>
        /// Limit of the maximum loaded items in the Active Tab
        /// </summary>
        public int MaxLoadedNFTItems { get; set; } = 40;

        /// <summary>
        /// This is same function as in NeblioAcocuntBase - TODO merge them to one common function
        /// This will make loading of the tab much faster
        /// </summary>
        /// <returns></returns>
        public async Task TxCashPreload()
        {
            // cash preload just for the NFT utxos?
            //var nftutxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address, NFTHelpers.AllowedTokens, new GetAddressInfoResponse() { Utxos = Utxos });

            Console.WriteLine("Cash of the TxInfo preload started...");

            var utxos = UtxosList.ToArray();
            var utxos_segment = new ArraySegment<Utxos>(utxos, 0, utxos.Length > MaxLoadedNFTItems ? MaxLoadedNFTItems : utxos.Length);

            if (utxos_segment != null && utxos_segment.Count > 1)
            {
                var txinfotasks = new ConcurrentQueue<Task>();
                foreach (var utxo in utxos_segment)
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
                /*
                Parallel.ForEach(new ArraySegment<Utxos>(utxos, 0, utxos.Length > MaxLoadedNFTItems ? MaxLoadedNFTItems : utxos.Length), new ParallelOptions { MaxDegreeOfParallelism = 10 }, utxo =>
                {
                    NeblioTransactionHelpers.GetTransactionInfo(utxo.Txid).Wait();//this cause the trouble
                    var tokid = utxo.Tokens?.FirstOrDefault()?.TokenId;
                    if (!string.IsNullOrEmpty(tokid) && !VEDLDataContext.NFTCache.ContainsKey(utxo.Txid))
                        NeblioTransactionHelpers.GetTokenMetadataOfUtxoCache(tokid, utxo.Txid).Wait(); //this cause the trouble
                    System.Threading.Thread.Sleep(20);
                });*/
            }
            Console.WriteLine("Cash of the TxInfo preload end...");
        }

        /// <summary>
        /// Start Automated refreshing
        /// </summary>
        /// <param name="interval">basic refresh interval</param>
        /// <param name="withoutMessages">dont load the NFT Messages</param>
        /// <param name="withCahePreload">Activate preload</param>
        /// <returns></returns>
        public async Task StartRefreshing(double interval = 5000, bool withoutMessages = true, bool withCahePreload = true)
        {
            Selected = true;
            FirsLoadingStatus?.Invoke(this, "Start Loading the data.");
            withoutMsgs = withoutMessages;
            cachePreload = withCahePreload;

            try
            {
                await Reload(withoutMsgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot Reload Active Tab after the start. " + ex.Message);
            }

            refreshTimer.Interval = interval;
            refreshTimer.AutoReset = true;
            refreshTimer.Elapsed -= RefreshTimer_Elapsed;
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Enabled = true;
            IsRefreshingRunning = true;
        }

        /// <summary>
        /// Stop automated refreshing
        /// </summary>
        /// <returns></returns>
        public void StopRefreshing()
        {
            IsRefreshingRunning = false;
            Selected = false;
            refreshTimer.Stop();
            refreshTimer.Enabled = false;
            refreshTimer.Elapsed -= RefreshTimer_Elapsed;
        }

        private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            refreshTimer.Stop();
            Reload(withoutMsgs);
        }

        /// <summary>
        /// Reload Address Utxos and refresh the NFT list
        /// </summary>
        /// <returns></returns>
        public async Task Reload(bool withoutMessages = true)
        {
            try
            {
                UtxosList = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address, NFTHelpers.AllowedTokens);
                if (NFTs.Count == 0 && cachePreload)
                {
                    try
                    {
                        await TxCashPreload();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot finish the preload." + ex.Message);
                    }
                }

                List<INFT> ns;
                lock (_lock)
                {
                    ns = NFTs.ToList();
                }
                NFTHelpers.ProfileNFTFound += NFTHelpers_ProfileNFTFound;
                NFTHelpers.NFTLoadingStateChanged += NFTHelpers_LoadingStateChangedHandler;
                var _NFTs = await NFTHelpers.LoadAddressNFTs(Address, UtxosList, ns, false, MaxLoadedNFTItems, withoutMessages);
                NFTHelpers.NFTLoadingStateChanged -= NFTHelpers_LoadingStateChangedHandler;
                NFTHelpers.ProfileNFTFound -= NFTHelpers_ProfileNFTFound;
                FirsLoadingStatus?.Invoke(this, "NFTs Loaded.");
                if (_NFTs != null)
                {
                    /*
                    if (_NFTs.Count < UtxosList.Count && MaxLoadedNFTItems < UtxosList.Count)
                    {
                        MaxLoadedNFTItems += 10;
                        CanLoadMore = true;
                    }
                    else
                    {
                        CanLoadMore = false;
                    }*/
                    lock (_lock)
                    {
                        NFTs = new List<INFT>(_NFTs);
                    }
                }

                if (_NFTs.Count > 0)
                    Profile = NFTHelpers.FindProfileNFT(_NFTs);

                FirsLoadingStatus?.Invoke(this, "Searching for NFT Payments.");
                await RefreshAddressReceivedPayments();

                NFTsChanged?.Invoke(this, Address);
                FirsLoadingStatus?.Invoke(this, "Loading finished.");
                refreshTimer?.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reload the tab content. " + ex.Message);
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

                    if ((firstpnft != null && ffirstpnft != null) || firstpnft == null && ffirstpnft != null)
                    {
                        if ((firstpnft == null && ffirstpnft != null) || (firstpnft != null && (firstpnft.Utxo != ffirstpnft.Utxo)))
                        {
                            ReceivedPayments.Clear();
                            foreach (var p in pnfts)
                            {
                                ReceivedPayments.TryAdd(p.NFTOriginTxId, p);
                                if (NFTs.Where(nft => NFTHelpers.IsBuyableNFT(nft.Type))
                                        .FirstOrDefault(n => n.Utxo == (p as PaymentNFT).NFTUtxoTxId &&
                                                             n.UtxoIndex == (p as PaymentNFT).NFTUtxoIndex) != null)
                                {
                                    NFTAddedToPayments?.Invoke(Address, ((p as PaymentNFT).NFTUtxoTxId, (p as PaymentNFT).NFTUtxoIndex));
                                }
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
        /// Load Bookmark data into ActiveTab class when some bookmark is found in another storage (for example NeblioAccount class)
        /// </summary>
        /// <param name="bkm"></param>
        public void LoadBookmark(Bookmark bkm)
        {
            if (!string.IsNullOrEmpty(bkm.Address) && !string.IsNullOrEmpty(bkm.Name))
            {
                IsInBookmark = true;
                BookmarkFromAccount = bkm;
            }
            else
                ClearBookmark();
        }
        /// <summary>
        /// Clear the Bookmark data in ActiveTab
        /// </summary>
        public void ClearBookmark()
        {
            IsInBookmark = false;
            BookmarkFromAccount = new Bookmark();
        }
    }
}
