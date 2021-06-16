using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.NFT;

namespace VEDriversLite.NFT.Coruzant
{
    public class CoruzantTab
    {
        public CoruzantTab(string address)
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
        public CoruzantProfileNFT Profile { get; set; } = new CoruzantProfileNFT("");

        public async Task Reload()
        {
            NFTs = await NFTHelpers.LoadAddressNFTs(Address, null, NFTs.ToList());
            if (NFTs == null)
                NFTs = new List<INFT>();

            if (NFTs.Count > 0)
                Profile = await CoruzantNFTHelpers.FindCoruzantProfileNFT(NFTs);

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
