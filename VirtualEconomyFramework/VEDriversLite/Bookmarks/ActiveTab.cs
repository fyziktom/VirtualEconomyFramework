using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
        public ProfileNFT Profile { get; set; } = new ProfileNFT("");

        public async Task Reload()
        {
            NFTs = await NFTHelpers.LoadAddressNFTs(Address);
            if (NFTs == null)
                NFTs = new List<INFT>();

            if (NFTs.Count > 0)
            {
                Profile = await NFTHelpers.FindProfileNFT(NFTs);
            }
        }
    }
}
