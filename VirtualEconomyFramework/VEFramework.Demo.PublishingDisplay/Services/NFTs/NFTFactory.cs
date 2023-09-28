using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEFramework.Demo.PublishingDisplay.Services.NFTs.Coruzant;
using VEDriversLite.NeblioAPI;
using System.Collections.Concurrent;
using VEFramework.Demo.PublishingDisplay.Services.NFTs.Dtos;

namespace VEFramework.Demo.PublishingDisplay.Services.NFTs
{
    /// <summary>
    /// Main factory for creating the NFTs from hash of the transaction
    /// </summary>
    public static class NFTFactory
    {
        /// <summary>
        /// NFT Hashes list. Usually it should contains list of all available NFTs in your app
        /// It should just speed up the testing
        /// </summary>
        public static ConcurrentDictionary<string, NFTHash> NFTHashs = new ConcurrentDictionary<string, NFTHash>();
        /// <summary>
        /// Public list of the NFT Cache
        /// </summary>
        public static ConcurrentDictionary<string, NFTCacheDto> NFTCache = new ConcurrentDictionary<string, NFTCacheDto>();
        
        /// <summary>
        /// Parse the type of the NFT from the transaction metadata
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static NFTTypes ParseNFTType(IDictionary<string,string> metadata)
        {
            NFTTypes type = NFTTypes.Image;

            if (metadata.TryGetValue("Type", out var t))
            {
                if (!string.IsNullOrEmpty(t))
                {
                    switch (t)
                    {

                        case "NFT CoruzantArticle":
                            type = NFTTypes.CoruzantArticle;
                            break;
                        case "NFT CoruzantPost":
                            type = NFTTypes.CoruzantArticle;
                            break;
                        case "NFT CoruzantPremiumArticle":
                            type = NFTTypes.CoruzantPremiumArticle;
                            break;
                        case "NFT CoruzantPodcast":
                            type = NFTTypes.CoruzantPodcast;
                            break;
                        case "NFT CoruzantPremiumPodcast":
                            type = NFTTypes.CoruzantPremiumPodcast;
                            break;
                        case "NFT CoruzantProfile":
                            type = NFTTypes.CoruzantProfile;
                            break;
                    }
                    return type;
                }
            }

            throw new Exception("Metadata does not contain NFT Type.");
        }
        /// <summary>
        /// Create and load the NFT from the transaction hash
        /// </summary>
        /// <param name="tokenId">Optional. If you know please provide to speed up the loading.</param>
        /// <param name="utxo">NFT transaction hash</param>
        /// <param name="utxoindex">Index of the NFT output. This is important for NFTs from multimint.</param>
        /// <param name="time">already parsed time</param>
        /// <param name="wait">await load - obsolete</param>
        /// <param name="loadJustType">load just specific type of NFT</param>
        /// <param name="justType">specify the type of the NFT which can be load - you must turn on justType flag</param>
        /// <param name="skipTheType">skip some type of NFT</param>
        /// <param name="skipType">specify the type of NFT which should be skipped - you must turn on skipTheType flag</param>
        /// <param name="address">Specify address of owner</param>
        /// <param name="txinfo">if you have loaded txinfo provide it to speed up the loading</param>
        /// <returns>INFT compatible object</returns>
        public static async Task<INFT> GetNFT(string tokenId,
                                              string utxo,
                                              int utxoindex = 0,
                                              double time = 0,
                                              bool wait = false,
                                              bool loadJustType = false,
                                              NFTTypes justType = NFTTypes.Image,
                                              bool skipTheType = false,
                                              NFTTypes skipType = NFTTypes.Image,
                                              string address = "",
                                              bool allowCache = false,
                                              int maxCachedItems = 100,
                                              VEDriversLite.NeblioAPI.GetTransactionInfoResponse? txinfo = null)
        {
            NFTTypes type = NFTTypes.Image;
            INFT nft = null;

            if (txinfo == null)
                txinfo = await NeblioAPIHelpers.GetTransactionInfo(utxo);
            if (txinfo == null)
                return null;
            if (txinfo.Vout == null)
                return null;
            var tokid = tokenId;
            try
            {
                var tid = txinfo.Vout.ToList()[utxoindex]?.Tokens.ToList()[0];
                if (tid == null) return null;
                if (tid.Amount > 1) return null;
                
                tokid = tid.TokenId;
                if (string.IsNullOrEmpty(tokid))
                    tokid = NFTHelpers.TokenId;
            }
            catch
            {
                return null;
            }

            if (allowCache && tokid == NFTHelpers.TokenId)
            {
                // try to load it from cache
                try
                {
                    if (NFTCache.TryGetValue($"{utxo}:{utxoindex}", out var cachedMetadata))
                    {
                        cachedMetadata.LastAccess = DateTime.UtcNow;
                        cachedMetadata.NumberOfReads++;
                        Console.WriteLine($"Loading {utxo}:{utxoindex} NFT from cache.");
                        if (cachedMetadata.NFTType == NFTTypes.Receipt)
                            Console.WriteLine($"NFT Receipt");
                        nft = await GetNFTFromCacheMetadata(cachedMetadata.Metadata, utxo, utxoindex, txinfo, true, cachedMetadata.NFTType);
                        if (nft != null)
                            Console.WriteLine($"Loading {utxo}:{utxoindex} NFT from cache done.");
                        if (nft != null)
                            return nft;
                        else
                            Console.WriteLine($"Loading {utxo}:{utxoindex} NFT from cache was not possible!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot load NFT from cache. " + ex.Message);
                    return null;
                }
            }

            var meta = await NeblioAPIHelpers.GetTransactionMetadata(tokid, utxo);

            if (meta == null)
                return null;
            else if (meta.Count == 0 || meta.Count == 1)
                return null;


            try
            {
                if (!meta.TryGetValue("NFT", out var nftt))
                    return null;
                else
                    if (nftt != "true") return null;

                type = ParseNFTType(meta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot load Type of NFT {utxo}. " + ex.Message);
                if (meta.TryGetValue("SourceUtxo", out var sourceutxo))
                    type = NFTTypes.Image;
                else
                    return null;
            }

            if (utxoindex == 1 && meta.TryGetValue("ReceiptFromPaymentUtxo", out var rfp))
                if (!string.IsNullOrEmpty(rfp))
                    type = NFTTypes.Receipt;

            if (loadJustType)
                if (justType != type)
                    return null;
            if (skipTheType)
                if (skipType == type || (skipType == NFTTypes.Message && type == NFTTypes.IoTMessage))
                    return null;

            var Time = TimeHelpers.UnixTimestampToDateTime(time);

            switch (type)
            {
                case NFTTypes.CoruzantArticle:
                    nft = new CoruzantArticleNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as CoruzantArticleNFT).LoadLastData(meta);
                    else
                        (nft as CoruzantArticleNFT).LoadLastData(meta);
                    nft.ParsePrice(meta);
                    break;
                case NFTTypes.CoruzantProfile:
                    nft = new CoruzantProfileNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as CoruzantProfileNFT).LoadLastData(meta);
                    else
                        (nft as CoruzantProfileNFT).LoadLastData(meta);
                    break;
            }

            if (allowCache && tokid == NFTHelpers.TokenId && NFTCache.Count < maxCachedItems)
            {
                if (nft.Type != NFTTypes.IoTDevice)
                {
                    try
                    {
                        var mtd = await nft.GetMetadata();
                        if (!NFTCache.TryGetValue($"{nft.Utxo}:{nft.UtxoIndex}", out var m))
                        {
                            NFTCache.TryAdd($"{nft.Utxo}:{nft.UtxoIndex}", new NFTCacheDto()
                            {
                                Address = address,
                                NFTType = nft.Type,
                                Metadata = mtd,
                                Utxo = nft.Utxo,
                                UtxoIndex = nft.UtxoIndex,
                                NumberOfReads = 0,
                                LastAccess = DateTime.UtcNow,
                                FirstSave = DateTime.UtcNow
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cannot load NFT {utxo} of type {nft.TypeText} to NFTCache dictionary. " + ex.Message);
                    }
                }
            }

            return nft;
        }

        /// <summary>
        /// Clone the NFT
        /// </summary>
        /// <param name="NFT">input NFT to clone</param>
        /// <param name="asType">Force the type of the output NFT</param>
        /// <param name="type">specify the type - you must turn on asType flag</param>
        /// <returns>INFT compatible object cloned from source</returns>
        public static async Task<INFT> CloneNFT(INFT NFT, bool asType = false, NFTTypes type = NFTTypes.Image)
        {
            if (!asType)
                type = NFT.Type;

            INFT nft = null;
            switch (type)
            {
                case NFTTypes.CoruzantArticle:
                    nft = new CoruzantArticleNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.CoruzantProfile:
                    nft = new CoruzantProfileNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
            }

            return null;
        }

        /// <summary>
        /// Load the NFT based on the data from the cache
        /// </summary>
        /// <param name="metadata">Metadata from cache of NFTs</param>
        /// <param name="utxo">Utxo of the NFT</param>
        /// <param name="utxoindex">Utxo Index of the NFT</param>
        /// <param name="txinfo">preloaded txinfo</param>
        /// <param name="asType">Force the output type</param>
        /// <param name="type">Specify the output type - you must set asType flag</param>
        /// <returns>INFT compatible object</returns>
        public static async Task<INFT> GetNFTFromCacheMetadata(IDictionary<string,string> metadata, string utxo, int utxoindex, VEDriversLite.NeblioAPI.GetTransactionInfoResponse? txinfo = null, bool asType = false, NFTTypes type = NFTTypes.Image)
        {
            if (!asType)
            {
                try
                {
                    type = ParseNFTType(metadata);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot load Type of NFT from cache. " + ex.Message);    
                    return null;
                }
            }

            if (type == NFTTypes.IoTDevice) return null;

            if (utxoindex == 1 && metadata.TryGetValue("ReceiptFromPaymentUtxo", out var rfp))
                if (!string.IsNullOrEmpty(rfp))
                    type = NFTTypes.Receipt;

            if (txinfo == null)
            {
                txinfo = await NeblioAPIHelpers.GetTransactionInfo(utxo);
                if (txinfo == null)
                    return null;
                if (txinfo.Vout == null)
                    return null;
            }
            var tokid = string.Empty;
            try
            {
                var tid = txinfo.Vout.ToList()[utxoindex]?.Tokens.ToList()[0]?.TokenId;
                tokid = tid;
                if (string.IsNullOrEmpty(tokid))
                {
                    tokid = NFTHelpers.TokenId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot parse token Id from NFT. " + ex.Message);
                return null;
            }

            INFT nft = null;
            switch (type)
            {

                case NFTTypes.CoruzantArticle:
                    nft = new CoruzantArticleNFT(utxo);
                    break;
                case NFTTypes.CoruzantProfile:
                    nft = new CoruzantProfileNFT(utxo);
                    break;
            }

            if (nft != null)
            {
                nft.Time = TimeHelpers.UnixTimestampToDateTime((double)txinfo.Blocktime);
                nft.Utxo = utxo;
                nft.UtxoIndex = utxoindex;
                nft.TxDetails = txinfo;
                nft.TokenId = tokid;
                nft.IsLoaded = true;
                await nft.LoadLastData(metadata);

                return nft;
            }
            else
            {
                return null;
            }
        }
    }
}
