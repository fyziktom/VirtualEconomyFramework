using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEFramework.Demo.PublishingDisplay.Services.NFTs;
using VEFramework.Demo.PublishingDisplay.Services.NFTs.Coruzant;
using VEDriversLite;
using Newtonsoft.Json;
using VEDriversLite.NeblioAPI;
using System.Collections.Concurrent;
using Dasync.Collections;

namespace VEFramework.Demo.PublishingDisplay.Services
{
    public class AppData
    {
        private static object _lock { get; set; } = new object();
        public static ConcurrentDictionary<string,INFT> NFTsDict { get; set; } = new ConcurrentDictionary<string, INFT>();
        public static ConcurrentDictionary<string,INFT> FoundingFellowsNFTsDict { get; set; } = new ConcurrentDictionary<string, INFT>();
        public static List<INFT> NFTs { get; set; } = new List<INFT>();
        public static List<INFT> FoundingFellowsNFTs { get; set; } = new List<INFT>();
        public static string Address { get; } = "NfzBf8eeqJ71zHf29npwYwEPkiWYZZtabJ"; // main coruzat publishing address
        public static string FoundingFellowsAddress { get; } = "NXRX9YA8sgfhaqASPd9Dv7eG9CEk4RqtZj"; // fellows nfts coruzat publishing address
        public static int MaxLoaded { get; set; } = 20;
        public static bool Loading { get; set; } = false;
        public static bool LoadingFellows { get; set; } = false;
        public static bool LoadedBase { get; set; } = false;
        public static bool Loaded { get; set; } = false;
        public static bool LoadedFellows { get; set; } = false;
        
        public IEnumerable<INFT> ArticleNFTs {
            get => NFTs.Where(n => n.Type == NFTTypes.CoruzantArticle);
        }
        
        private static Random rnd = new Random();
        public IEnumerable<INFT> RandArticleNFTs {
            get => NFTs.Where(n => n.Type == NFTTypes.CoruzantArticle).OrderBy(n => rnd.Next());
        }
        public IEnumerable<INFT> ProfilesNFTs
        {
            get => NFTs.Where(n => n.Type == NFTTypes.CoruzantProfile);
        }
        public IEnumerable<INFT> PodcastsNFTs
        {
            get => NFTs.Where(n => n.Type == NFTTypes.CoruzantProfile).Where(n => !string.IsNullOrEmpty((n as CoruzantProfileNFT).PodcastId));
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
            var utxos = await NeblioAPIHelpers.GetAddressNFTsUtxos(Address, new List<string>() { NFTHelpers.CoruzantTokenId });
            var initcount = 3;

            //await new ArraySegment<Utxos>(utxos.ToArray(), 0, initcount).ParallelForEachAsync(async u =>
            //{
            //    var nft = await NFTFactory.GetNFT(NFTHelpers.CoruzantTokenId, u.Txid, (int)(u.Index ?? 0), (double)(u.Blocktime ?? 0.0), address: Address, wait:true);
            //    if (nft != null)
            //        NFTsDict.TryAdd($"{u.Txid}:{u.Index}", nft);
            //}, maxDegreeOfParallelism:10);
            var i = 0;
            foreach (var u in utxos)
            {
                var nft = await NFTFactory.GetNFT(NFTHelpers.CoruzantTokenId, u.Txid, (int)(u.Index ?? 0), (double)(u.Blocktime ?? 0.0), address: Address, wait: true);
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
                    var nft = await NFTFactory.GetNFT(NFTHelpers.CoruzantTokenId, u.Txid, (int)u.Index, (double)u.Blocktime, address: Address);
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

        public async Task LoadFoundingFellows()
        {
            lock (_lock)
            {
                LoadingFellows = true;
                LoadedFellows = false;
                FoundingFellowsNFTs.Clear();
            }
            Console.WriteLine("Start loading NFTs Fellows.");
            var utxos = await NeblioAPIHelpers.GetAddressNFTsUtxos(FoundingFellowsAddress, new List<string>() { NFTHelpers.CoruzantTokenId });

            await new ArraySegment<Utxos>(utxos.ToArray(), 0, utxos.Count).ParallelForEachAsync(async u =>
            {
                var nft = await NFTFactory.GetNFT(NFTHelpers.CoruzantTokenId, u.Txid, (int)u.Index, (double)u.Blocktime, address: Address);
                if (nft != null)
                    FoundingFellowsNFTsDict.TryAdd($"{u.Txid}:{u.Index}", nft);
            }, maxDegreeOfParallelism: 4);
            
            lock (_lock)
            {
                FoundingFellowsNFTs = FoundingFellowsNFTsDict.Values.OrderBy(n => n.Time).Reverse().ToList();
                LoadedFellows = true;
                LoadingFellows = false;
            }
            
            Console.WriteLine("All NFTs Fellows loaded.");
            Console.WriteLine("Total Fellows loaded." + FoundingFellowsNFTs.Count.ToString());
            NFTsFellowsLoaded?.Invoke(this, new EventArgs());
        }

        public async Task LoadMoreNFTs(int moreCount = 10)
        {
            lock (_lock)
            {
                Loading = true;
            }
            Console.WriteLine("Start loading additional NFTs.");
            var utxosin = await NeblioAPIHelpers.GetAddressNFTsUtxos(Address, new List<string>() { NFTHelpers.CoruzantTokenId });
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
                var nft = await NFTFactory.GetNFT(NFTHelpers.CoruzantTokenId, u.Txid, (int)u.Index, (double)u.Blocktime, address: Address);
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
