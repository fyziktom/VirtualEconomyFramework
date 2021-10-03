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
    public class ActiveTab
    {
        private static object _lock = new object();
        public ActiveTab() { }
        public ActiveTab(string address)
        {
            Address = address;
            ShortAddress = NeblioTransactionHelpers.ShortenAddress(address);
        }

        [JsonIgnore]
        public bool Selected { get; set; } = false;
        public string Address { get; set; } = string.Empty;
        public string ShortAddress { get; set; } = string.Empty;
        public bool IsInBookmark { get; set; } = false;
        
        public bool CanLoadMore { get; set; } = false;
        [JsonIgnore]
        public bool IsRefreshingRunning { get; set; } = false;
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        [JsonIgnore]
        public ICollection<Utxos> UtxosList { get; set; } = new List<Utxos>();
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments = new ConcurrentDictionary<string, INFT>();
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


        private System.Timers.Timer refreshTimer = new System.Timers.Timer();
        public int MaxLoadedNFTItems { get; set; } = 40;

        public async Task StartRefreshing(double interval = 5000)
        {
            Selected = true;
            FirsLoadingStatus?.Invoke(this, "Start Loading the data.");
            await Reload();
            refreshTimer.Interval = interval;
            refreshTimer.AutoReset = true;
            refreshTimer.Elapsed -= RefreshTimer_Elapsed;
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Enabled = true;
            IsRefreshingRunning = true;
        }

        public async Task StopRefreshing()
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
            Reload();
        }

        public async Task Reload()
        {
            try
            {
                UtxosList = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address, NFTHelpers.AllowedTokens);
                List<INFT> ns;
                lock (_lock)
                {
                    ns = NFTs.ToList();
                }
                NFTHelpers.ProfileNFTFound += NFTHelpers_ProfileNFTFound;
                NFTHelpers.NFTLoadingStateChanged += NFTHelpers_LoadingStateChangedHandler;
                var _NFTs = await NFTHelpers.LoadAddressNFTs(Address, UtxosList, ns, false, MaxLoadedNFTItems, true);
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
                    Profile = await NFTHelpers.FindProfileNFT(_NFTs);

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
        public void ClearBookmark()
        {
            IsInBookmark = false;
            BookmarkFromAccount = new Bookmark();
        }
    }
}
