using Dasync.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Accounts.Dto;
using VEDriversLite.Dto;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.Accounts.NFTModules
{
    public abstract class CommonNFTModule : INFTModule
    {
        private static object _lock { get; set; } = new object();

        /// <summary>
        /// Address which runs this Module
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Unique Id of the module
        /// </summary>
        public string Id { get; set; } = string.Empty;
        public NFTModuleType Type { get; set; } = NFTModuleType.VENFT;
        public string TokenId { get; set; } = string.Empty;
        public double SourceTokensBalance { get; set; } = 0.0;
        public int NFTCount { get; set; } = 0;
        public int MaximumOfLoadedNFTs { get; set; } = 0;
        public List<INFT> NFTs { get => NFTsDict.Values.OrderBy(n => n.Time)?.Reverse()?.ToList(); }
        public ConcurrentDictionary<string, INFT> NFTsDict { get; set; } = new ConcurrentDictionary<string, INFT>();
        public TokenSupplyDto TokensSupply { get; set; } = new TokenSupplyDto();

        /// <summary>
        /// This event is fired whenever the list of NFTs is changed
        /// </summary>
        public virtual event EventHandler<string> NFTsChanged;
        /// <summary>
        /// This event is fired whenever the list of NFTs is changed during the first load when all NFTs are loaded
        /// </summary>
        public virtual event EventHandler<INFT> FirstLoadNFTsChanged;
        /// <summary>
        /// <summary>
        /// This event is called during first loading of the account to keep updated the user
        /// </summary>
        public virtual event EventHandler<string> FirsLoadingStatus;

        /// <summary>
        /// Init the NFT Module
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public virtual async Task Init(string address, string id = "")
        {
            if (!string.IsNullOrEmpty(address))
                Address = address;
            if (!string.IsNullOrEmpty(id))
                Id = id;
            else
                Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// This function will reload changes in the NFTs list based on provided list of already loaded utxos.
        /// </summary>
        /// <returns></returns>
        public virtual async Task ReLoadNFTs(ReloadNFTSetting settings)
        {
            if (string.IsNullOrEmpty(settings.Address) && string.IsNullOrEmpty(Address))
                throw new Exception("Cannot Reload NFTs without main Address of the NFT Module");

            try
            {
                var nftutxos = settings.Utxos.Where(u => (u.Value == 10000 && u.Tokens.Count > 0))
                                             .Where(u => u.Tokens[0]?.TokenId == TokenId && u.Tokens[0]?.Amount == 1)
                                             .ToArray();

                if (nftutxos == null || nftutxos.Count() == 0)
                    return;

                var fireProfileEventTmp = true;
                var lastnftsCount = NFTsDict.Count();
                ArraySegment<Utxo> nftus = null;
                if (settings.MaxItems > 0 && nftutxos.Length > settings.MaxItems)
                    nftus = new ArraySegment<Utxo>(nftutxos, 0, settings.MaxItems);
                else
                    nftus = nftutxos;

                var nftutxosCount = nftus.Count();

                if (NFTsDict.Count > 0)
                {
                    // remove old ones
                    foreach (var n in NFTsDict.Values)
                        if (nftus.FirstOrDefault(nu => nu.Txid == n.Utxo && nu.Index == n.UtxoIndex) == null)
                        {
                            NFTsDict.TryRemove($"{n.Utxo}:{n.UtxoIndex}", out var nft);
                            NFTsChanged?.Invoke(this, "Changed");
                        }
                }

                await nftus.ParallelForEachAsync(async n =>
                {
                    if (settings.MaxItems == 0 || (settings.MaxItems > 0 && NFTsDict.Count < settings.MaxItems))
                    {
                        var tok = n.Tokens.FirstOrDefault();
                        if (!NFTsDict.TryGetValue($"{n.Txid}:{n.Index}", out var nft))
                        {
                            try
                            {
                                nft = await NFTFactory.GetNFT(tok.TokenId,
                                                              n.Txid,
                                                              n.Index,
                                                              n.Time,
                                                              address: settings.Address,
                                                              loadJustTypes: settings.LoadJustTypes,
                                                              skipTypes: settings.SkipTypes);
                                if (nft != null)
                                {
                                    NFTsDict.TryAdd($"{n.Txid}:{n.Index}", nft);

                                    var count = settings.MaxItems > 0 ? settings.MaxItems : nftutxosCount;
                                    if (settings.FirstLoad)
                                    {
                                        FirsLoadingStatus?.Invoke(settings.Address, $"Loaded {NFTsDict.Count} NFT of {count}.");
                                        FirstLoadNFTsChanged?.Invoke(this, nft);
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine("Cannot load the NFT: " + n.Txid);
                            }
                        }
                    }
                }, maxDegreeOfParallelism: 10);

                if (settings.FirstLoad && NFTsDict.Count != lastnftsCount)
                    NFTsChanged?.Invoke(this, "Changed");

                NFTCount = NFTsDict.Count;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot reload NFTs. " + ex.Message);
            }
        }

        public virtual async Task<(bool, INFT)> GetNFTIfExists(string utxo, int index)
        {
            if (string.IsNullOrEmpty(utxo))
                return (false, null);
            if (NFTsDict.TryGetValue($"{utxo}:{index}", out var nft))
                return (true, nft);
            else
                return (false, null);
        }
    }
}
