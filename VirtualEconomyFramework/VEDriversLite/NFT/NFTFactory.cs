using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT.Coruzant;

namespace VEDriversLite.NFT
{
    public static class NFTFactory
    {
        public static NFTTypes ParseNFTType(IDictionary<string,string> metadata)
        {
            NFTTypes type = NFTTypes.Image;

            if (metadata.TryGetValue("Type", out var t))
            {
                if (!string.IsNullOrEmpty(t))
                {
                    switch (t)
                    {
                        case "NFT Profile":
                            type = NFTTypes.Profile;
                            break;
                        case "NFT Post":
                            type = NFTTypes.Post;
                            break;
                        case "NFT Image":
                            type = NFTTypes.Image;
                            break;
                        case "NFT Payment":
                            type = NFTTypes.Payment;
                            break;
                        case "NFT Music":
                            type = NFTTypes.Music;
                            break;
                        case "NFT Message":
                            type = NFTTypes.Message;
                            break;
                        case "NFT Ticket":
                            type = NFTTypes.Ticket;
                            break;
                        case "NFT Event":
                            type = NFTTypes.Event;
                            break;
                        case "NFT CoruzantArticle":
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
                                              NeblioAPI.GetTransactionInfoResponse txinfo = null)
        {
            NFTTypes type = NFTTypes.Image;
            INFT nft = null;

            if (txinfo == null)
                txinfo = await NeblioTransactionHelpers.GetTransactionInfo(utxo);
            if (txinfo == null)
                return null;
            if (txinfo.Vout == null)
                return null;
            var tokid = tokenId;
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
                return null;
            }

            if (VEDLDataContext.AllowCache && tokid == NFTHelpers.TokenId)
            {
                // try to load it from cache
                try
                {
                    if (VEDLDataContext.NFTCache.TryGetValue($"{utxo}:{utxoindex}", out var cachedMetadata))
                    {
                        cachedMetadata.LastAccess = DateTime.UtcNow;
                        cachedMetadata.NumberOfReads++;
                        nft = await GetNFTFromCacheMetadata(cachedMetadata.Metadata, utxo, utxoindex, txinfo, true, cachedMetadata.NFTType);
                        if (nft != null)
                            return nft;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot load NFT from cache. " + ex.Message);
                    return null;
                }
            }

            var meta = await NeblioTransactionHelpers.GetTransactionMetadata(tokid, utxo);

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
                {
                    type = NFTTypes.Image;
                }
                else
                {
                    return null;
                }
            }

            if (utxoindex == 1 && meta.TryGetValue("ReceiptFromPaymentUtxo", out var rfp))
            {
                if (!string.IsNullOrEmpty(rfp))
                {
                    type = NFTTypes.Receipt;
                }
            }

            if (loadJustType)
                if (justType != type)
                    return null;
            if (skipTheType)
                if (skipType == type)
                    return null;

            var Time = TimeHelpers.UnixTimestampToDateTime(time);

            var Price = 0.0;
            var PriceActive = false;
            if (meta.TryGetValue("Price", out var price))
                if (!string.IsNullOrEmpty(price))
                {
                    price = price.Replace(',', '.');
                    Price = double.Parse(price, CultureInfo.InvariantCulture);
                    PriceActive = true;
                }

            if (Price > 0)
                PriceActive = true;
            else
                PriceActive = false;

            switch (type)
            {
                case NFTTypes.Image:
                    nft = new ImageNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await nft.ParseOriginData(meta);
                    else
                        nft.ParseOriginData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
                case NFTTypes.Profile:
                    nft = new ProfileNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as ProfileNFT).LoadLastData(meta);
                    return nft;
                case NFTTypes.Post:
                    nft = new PostNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as PostNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Music:
                    nft = new MusicNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    //await ponft.ParseOriginData();
                    if (wait)
                        await nft.ParseOriginData(meta);
                    else
                        nft.ParseOriginData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
                case NFTTypes.Payment:
                    nft = new PaymentNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as PaymentNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Receipt:
                    nft = new ReceiptNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as ReceiptNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Message:
                    nft = new MessageNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as MessageNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Ticket:
                    nft = new TicketNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await nft.ParseOriginData(meta);
                    else
                        nft.ParseOriginData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
                case NFTTypes.Event:
                    nft = new EventNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await nft.ParseOriginData(meta);
                    else
                        nft.ParseOriginData(meta);
                    //await (nft as EventNFT).LoadLastData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
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

            if (VEDLDataContext.AllowCache && tokid == NFTHelpers.TokenId && VEDLDataContext.NFTCache.Count < VEDLDataContext.MaxCachedItems)
            {
                try
                {
                    var mtd = await nft.GetMetadata();
                    if (!VEDLDataContext.NFTCache.TryGetValue($"{nft.Utxo}:{nft.UtxoIndex}", out var m))
                    {
                        VEDLDataContext.NFTCache.TryAdd($"{nft.Utxo}:{nft.UtxoIndex}", new Dto.NFTCacheDto()
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

            return nft;
        }

        public static async Task<INFT> CloneNFT(INFT NFT, bool asType = false, NFTTypes type = NFTTypes.Image)
        {
            if (!asType)
                type = NFT.Type;

            INFT nft = null;
            switch (type)
            {
                case NFTTypes.Image:
                    nft = new ImageNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Profile:
                    nft = new ProfileNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Post:
                    nft = new PostNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Music:
                    nft = new MusicNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Payment:
                    nft = new PaymentNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Receipt:
                    nft = new ReceiptNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Message:
                    nft = new MessageNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Ticket:
                    nft = new TicketNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Event:
                    nft = new EventNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
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

        public static async Task<INFT> GetNFTFromCacheMetadata(IDictionary<string,string> metadata, string utxo, int utxoindex, NeblioAPI.GetTransactionInfoResponse txinfo = null, bool asType = false, NFTTypes type = NFTTypes.Image)
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

            if (utxoindex == 1 && metadata.TryGetValue("ReceiptFromPaymentUtxo", out var rfp))
            {
                if (!string.IsNullOrEmpty(rfp))
                {
                    type = NFTTypes.Receipt;
                }
            }

            if (txinfo == null)
            {
                txinfo = await NeblioTransactionHelpers.GetTransactionInfo(utxo);
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
                case NFTTypes.Image:
                    nft = new ImageNFT(utxo);
                    break;
                case NFTTypes.Profile:
                    nft = new ProfileNFT(utxo);
                    break;
                case NFTTypes.Post:
                    nft = new PostNFT(utxo);
                    break;
                case NFTTypes.Music:
                    nft = new MusicNFT(utxo);
                    break;
                case NFTTypes.Payment:
                    nft = new PaymentNFT(utxo);
                    break;
                case NFTTypes.Receipt:
                    nft = new ReceiptNFT(utxo);
                    break;
                case NFTTypes.Message:
                    nft = new MessageNFT(utxo);
                    break;
                case NFTTypes.Ticket:
                    nft = new TicketNFT(utxo);
                    break;
                case NFTTypes.Event:
                    nft = new EventNFT(utxo);
                    break;
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
                nft.UtxoIndex = utxoindex;
                nft.TxDetails = txinfo;
                nft.TokenId = tokid;
                nft.IsLoaded = true;
                await nft.LoadLastData(metadata);
                if (nft.Type == NFTTypes.Message)
                    (nft as MessageNFT).Decrypted = false;
                
                return nft;
            }
            else
            {
                return null;
            }
        }
    }
}
