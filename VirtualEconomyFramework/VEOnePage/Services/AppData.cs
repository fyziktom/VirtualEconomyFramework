using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEDriversLite.NFT;
using VEDriversLite.Neblio;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite;
using Newtonsoft.Json;
using VEDriversLite.NeblioAPI;
using System.Collections.Concurrent;
using Dasync.Collections;

namespace VEOnePage.Services
{
    public class AppData
    {
        private static object _lock { get; set; } = new object();
        public static ConcurrentDictionary<string,INFT> NFTsDict { get; set; } = new ConcurrentDictionary<string, INFT>();
        public static List<INFT> NFTs { get; set; } = new List<INFT>();
        public static string Address { get; } = "NfzBf8eeqJ71zHf29npwYwEPkiWYZZtabJ"; // main coruzat publishing address
        public static int MaxLoaded { get; } = 30;

        public List<INFT> ArticleNFTs {
            get => NFTs.Where(n => n.Type == NFTTypes.CoruzantArticle).ToList();
        }
        public List<INFT> ProfilesNFTs
        {
            get => NFTs.Where(n => n.Type == NFTTypes.CoruzantProfile).ToList();
        }
        public event EventHandler NFTsLoaded;

        public async Task LoadNFTs()
        {
            var utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(Address, new List<string>() { CoruzantNFTHelpers.CoruzantTokenId });

            await new ArraySegment<Utxos>(utxos.ToArray(), 0, MaxLoaded).ParallelForEachAsync(async u =>
            {
                var nft = await NFTFactory.GetNFT(CoruzantNFTHelpers.CoruzantTokenId, u.Txid, (int)u.Index, (double)u.Blocktime, address: Address);
                if (nft != null)
                    NFTsDict.TryAdd($"{u.Txid}:{u.Index}", nft);
            }, maxDegreeOfParallelism:10);

            lock (_lock)
            {
                NFTs = NFTsDict.Values.OrderBy(n => n.Time).Reverse().ToList();
            }

            Console.WriteLine("All NFTs loaded.");
            Console.WriteLine("Total loaded." + NFTs.Count.ToString());
            NFTsLoaded?.Invoke(this, null);
        }
    }
}
