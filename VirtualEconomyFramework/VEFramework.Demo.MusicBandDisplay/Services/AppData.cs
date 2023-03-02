using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEFramework.Demo.MusicBandDisplay.Services.NFTs;
using Newtonsoft.Json;
using VEDriversLite.NeblioAPI;
using System.Collections.Concurrent;
using Dasync.Collections;
using VEFramework.Demo.MusicBandDisplay.Services.NFTs.Tags;

namespace VEFramework.Demo.MusicBandDisplay.Services
{
    public class AppData
    {
        private static object _lock { get; set; } = new object();
        public static Dictionary<string, Tag> Tags { get; set; } = new Dictionary<string, Tag>();
        public static ConcurrentDictionary<string,INFT> NFTsDict { get; set; } = new ConcurrentDictionary<string, INFT>();
        public static List<INFT> NFTs { get; set; } = new List<INFT>();
        public static string Address { get; set; } = ""; // main publishing address
        public static string AppShareNFTUrl { get; set; } = "";
        public static int MaxLoaded { get; set; } = 20;
        public static bool Loading { get; set; } = false;
        public static bool LoadingFellows { get; set; } = false;
        public static bool LoadedBase { get; set; } = false;
        public static bool Loaded { get; set; } = false;
        public static bool LoadedFellows { get; set; } = false;
        
        public IEnumerable<INFT> MusicNFTs {
            get => NFTs.Where(n => n.Type == NFTTypes.Music);
        }
        
        private static Random rnd = new Random();
        public IEnumerable<INFT> RandMusicNFTs {
            get => NFTs.Where(n => n.Type == NFTTypes.Music).OrderBy(n => rnd.Next());
        }
        public IEnumerable<INFT> PostNFTs
        {
            get => NFTs.Where(n => n.Type == NFTTypes.Post);
        }
        public IEnumerable<INFT> RandPostsNFTs
        {
            get => NFTs.Where(n => n.Type == NFTTypes.Post).OrderBy(n => rnd.Next());
        }

        public event EventHandler? NFTsLoaded;
        public event EventHandler? NFTsFellowsLoaded;

        public async Task LoadNFTs()
        {
            lock (_lock)
            {
                Loading = true;
                LoadedBase = false;
                Loaded = false;
                NFTs.Clear();
            }
            Console.WriteLine("Start loading NFTs.");
            var utxos = await NeblioAPIHelpers.GetAddressNFTsUtxos(Address, new List<string>() { NFTHelpers.TokenId });
            var initcount = 3;

            var i = 0;
            foreach (var u in utxos)
            {
                var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, u.Txid, (int)(u.Index ?? 0), (double)(u.Blocktime ?? 0.0), address: Address, wait: true);
                if (nft != null)
                    NFTsDict.TryAdd($"{u.Txid}:{u.Index}", nft);
                i++;
                if (i > initcount)
                    break;

                lock (_lock)
                {
                    NFTs = NFTsDict.Values.OrderBy(n => n.Time).Reverse().ToList();
                }
                NFTsLoaded?.Invoke(this, new EventArgs());
            }
            lock (_lock)
            {
                LoadedBase = true;
            }

            if (utxos.Count > initcount && MaxLoaded > initcount)
            {
                await new ArraySegment<Utxos>(utxos.ToArray(), initcount, MaxLoaded).ParallelForEachAsync(async u =>
                {
                    var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, address: Address);
                    if (nft != null)
                        NFTsDict.TryAdd($"{u.Txid}:{u.Index}", nft);
                }, maxDegreeOfParallelism: 4);
            }
            lock (_lock)
            {
                NFTs = NFTsDict.Values.OrderBy(n => n.Time).Reverse().ToList();
                Loaded = true;
            }

            Console.WriteLine("All NFTs loaded.");
            Console.WriteLine("Total loaded." + NFTs.Count.ToString());
            lock (_lock)
            {
                Loading = false;
            }            
            NFTsLoaded?.Invoke(this, new EventArgs());
        }

        public async Task LoadMoreNFTs(int moreCount = 10)
        {
            lock (_lock)
            {
                Loading = true;
            }
            Console.WriteLine("Start loading additional NFTs.");
            var utxosin = await NeblioAPIHelpers.GetAddressNFTsUtxos(Address, new List<string>() { NFTHelpers.TokenId });
            MaxLoaded += moreCount;
            
            if (MaxLoaded >= utxosin.Count)
                MaxLoaded = utxosin.Count;

            var utxos = new List<Utxos>();
            foreach (var u in utxosin)
            {
                if (NFTsDict.ContainsKey($"{u.Txid}:{u.Index}"))
                    continue;
                else
                    utxos.Add(u);
                if (utxos.Count >= moreCount)
                    break;
            }
                        
            await new ArraySegment<Utxos>(utxos.ToArray(), 0, utxos.Count).ParallelForEachAsync(async u =>
            {
                var nft = await NFTFactory.GetNFT(NFTHelpers.TokenId, u.Txid, (int)u.Index, (double)u.Blocktime, address: Address);
                if (nft != null)
                    NFTsDict.TryAdd($"{u.Txid}:{u.Index}", nft);
            }, maxDegreeOfParallelism: 4);
            
            lock (_lock)
            {
                NFTs = NFTsDict.Values.OrderBy(n => n.Time).Reverse().ToList();
            }

            Console.WriteLine("All additional NFTs loaded.");
            Console.WriteLine("Total loaded." + NFTs.Count.ToString());
            lock (_lock)
            {
                Loading = false;
            }
            NFTsLoaded?.Invoke(this, new EventArgs());
        }
    }
}
