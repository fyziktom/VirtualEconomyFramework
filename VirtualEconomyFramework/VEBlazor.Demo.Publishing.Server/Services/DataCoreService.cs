using System.Collections.Concurrent;
using VEDriversLite;
using VEDriversLite.Bookmarks;
using VEDriversLite.Neblio;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Coruzant;

namespace VEBlazor.Demo.Publishing.Server.Services
{
    public static class MainDataContext
    {
        public static string MainCoruzantPublishingAddress { get; set; } = "NfzBf8eeqJ71zHf29npwYwEPkiWYZZtabJ";
        public static string MainCoruzantFellowsAddress { get; set; } = "NXRX9YA8sgfhaqASPd9Dv7eG9CEk4RqtZj";
        public static string MainCoruzantArticlesAddress { get; set; } = "NfzBf8eeqJ71zHf29npwYwEPkiWYZZtabJ";
        public static string MainCoruzantProfilesAddress { get; set; } = "NfzBf8eeqJ71zHf29npwYwEPkiWYZZtabJ";
        public static NeblioAccount MainAccount { get; set; } = new NeblioAccount();

        public static string IpfsSecret { get; set; } = ""; // fill your infura ipfs secret
        public static string IpfsProjectID { get; set; } = ""; // fill your infura ipfs project id
        public static Dictionary<string, INFT> NFTs = new Dictionary<string, INFT>();
        public static List<string> ObservedAccounts { get; set; } = new List<string>();
        public static ConcurrentDictionary<string, ActiveTab> ObservedAccountsTabs { get; set; } = new ConcurrentDictionary<string, ActiveTab>();
    }
    public class DataCoreService
    {
        
        public static async Task<INFT> GetNFT(string utxo, int index = 0)
        {
            var nft = await NFTFactory.GetNFT(CoruzantNFTHelpers.CoruzantTokenId, utxo, index, 0, true);
            return nft;
        }
    }
}
