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
    /// <summary>
    /// Coruzant tab loads the coruzant NFTs
    /// </summary>
    public class CoruzantTab
    {
        /// <summary>
        /// Create tab for the specific address which will be loaded
        /// Constructor will create also shorten version of the address to simplify load to the UI
        /// </summary>
        /// <param name="address"></param>
        public CoruzantTab(string address)
        {
            Address = address;
            ShortAddress = NeblioTransactionHelpers.ShortenAddress(address);
        }

        /// <summary>
        /// Tab is selected
        /// </summary>
        public bool Selected { get; set; } = false;
        /// <summary>
        /// Address loaded on this tab
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Shorten version of the address
        /// </summary>
        public string ShortAddress { get; set; } = string.Empty;
        /// <summary>
        /// Is set when tab is in the bookmarks of the main account
        /// </summary>
        public bool IsInBookmark { get; set; } = false;
        /// <summary>
        /// List of the NFTs loadef on this tab
        /// </summary>
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        /// <summary>
        /// Loaded bookmark object from the main account
        /// </summary>
        [JsonIgnore]
        public Bookmark BookmarkFromAccount { get; set; } = new Bookmark();
        /// <summary>
        /// Coruzant profile NFT        
        /// </summary>
        [JsonIgnore]
        public CoruzantProfileNFT Profile { get; set; } = new CoruzantProfileNFT("");
        
        /// <summary>
        /// Reload the data        
        /// </summary>
        /// <returns></returns>
        public async Task Reload()
        {
            NFTs = await NFTHelpers.LoadAddressNFTs(Address, null, NFTs.ToList());
            if (NFTs == null)
                NFTs = new List<INFT>();

            if (NFTs.Count > 0)
                Profile = await CoruzantNFTHelpers.FindCoruzantProfileNFT(NFTs);

        }

        /// <summary>
        /// Load bookmark object        
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
        /// Clear bookmark object
        /// </summary>
        public void ClearBookmark()
        {
            IsInBookmark = false;
            BookmarkFromAccount = new Bookmark();
        }
    }
}
