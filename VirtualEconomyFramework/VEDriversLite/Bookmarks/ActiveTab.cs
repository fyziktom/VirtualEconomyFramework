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

        private System.Timers.Timer refreshTimer = new System.Timers.Timer();
        private int MaxLoadedNFTItems = 40;

        public async Task StartRefreshing(double interval = 5000)
        {
            Selected = true;
            refreshTimer.Interval = interval;
            refreshTimer.AutoReset = true;
            refreshTimer.Elapsed -= RefreshTimer_Elapsed;
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Enabled = true;
            IsRefreshingRunning = true;
            await Reload();
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
                var _NFTs = await NFTHelpers.LoadAddressNFTs(Address, UtxosList, ns, false, MaxLoadedNFTItems, true);
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

                //await RefreshAddressReceivedPayments();
                NFTsChanged?.Invoke(this, Address);
                refreshTimer?.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reload the tab content. " + ex.Message);
            }
        }
        public async Task RefreshAddressReceivedPayments()
        {
            ReceivedPayments.Clear();
            var pnfts = NFTs.Where(n => n.Type == NFTTypes.Payment).ToList();
            if (pnfts.Count > 0)
                foreach (var p in pnfts)
                    ReceivedPayments.TryAdd(p.NFTOriginTxId, p);
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
