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
            ShortAddress = Address.Substring(0, 3) + "..." + Address.Substring(Address.Length - 3);
        }

        public bool Selected { get; set; } = false;
        public string Address { get; set; } = string.Empty;
        public string ShortAddress { get; set; } = string.Empty;
        [JsonIgnore]
        public List<INFT> NFTs { get; set; } = new List<INFT>();
    }
}
