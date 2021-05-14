using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT;

namespace VEDriversLite.Bookmarks
{
    public class ActiveTab
    {
        public ActiveTab(string address)
        {
            Address = address;
            ShortAddress = NeblioTransactionHelpers.ShortenAddress(address);
        }

        public bool Selected { get; set; } = false;
        public string Address { get; set; } = string.Empty;
        public string ShortAddress { get; set; } = string.Empty;
        public bool IsInBookmark { get; set; } = false;
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        [JsonIgnore]
        public ConcurrentDictionary<string, INFT> ReceivedPayments = new ConcurrentDictionary<string, INFT>();
        [JsonIgnore]
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");

        public async Task Reload()
        {
            NFTs = await NFTHelpers.LoadAddressNFTs(Address, null, NFTs.ToList());
            if (NFTs == null)
                NFTs = new List<INFT>();

            if (NFTs.Count > 0)
                Profile = await NFTHelpers.FindProfileNFT(NFTs);

            await RefreshAddressReceivedPayments();
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
