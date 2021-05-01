using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
