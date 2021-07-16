using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NFT;

namespace VENFTApp_Server
{
    public static class MainDataContext
    {
        public static string IpfsSecret { get; set; } = ""; // fill your infura ipfs secret
        public static string IpfsProjectID { get; set; } = ""; // fill your infura ipfs project id
        public static Dictionary<string, INFT> NFTs = new Dictionary<string, INFT>();
        public static Dictionary<string, TokenOwnerDto> VENFTTokenOwners = new Dictionary<string, TokenOwnerDto>();
        public static ConcurrentDictionary<string, string> UsedAddressesWithTickets = new ConcurrentDictionary<string, string>();
        public static bool IsAPIWithCredentials { get; set; } = false;
    }
}
