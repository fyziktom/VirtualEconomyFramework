using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT.Coruzant;
using VEDriversLite.NFT.DevicesNFTs;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Main factory for creating the NFTs from hash of the transaction
    /// </summary>
    public static class NFTFactory
    {
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
                        case "NFT Receipt":
                            type = NFTTypes.Receipt;
                            break;
                        case "NFT Invoice":
                            type = NFTTypes.Invoice;
                            break;
                        case "NFT Order":
                            type = NFTTypes.Order;
                            break;
                        case "NFT Product":
                            type = NFTTypes.Product;
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
                        case "NFT Device":
                            type = NFTTypes.Device;
                            break;
                        case "NFT IoTDevice":
                            type = NFTTypes.IoTDevice;
                            break;
                        case "NFT Protocol":
                            type = NFTTypes.Protocol;
                            break;
                        case "NFT HWSrc":
                            type = NFTTypes.HWSrc;
                            break;
                        case "NFT FWSrc":
                            type = NFTTypes.FWSrc;
                            break;
                        case "NFT SWSrc":
                            type = NFTTypes.SWSrc;
                            break;
                        case "NFT MechSrc":
                            type = NFTTypes.MechSrc;
                            break;
                        case "NFT IoTMessage":
                            type = NFTTypes.IoTMessage;
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
                    tokid = NFTHelpers.TokenId;
            }
            catch
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
                case NFTTypes.Image:
                    nft = new ImageNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    //if (wait)
                        await nft.ParseOriginData(meta);
                    //else
                        //nft.ParseOriginData(meta);
                    nft.ParsePrice(meta);
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
                    nft.ParsePrice(meta);
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
                case NFTTypes.Invoice:
                    nft = new InvoiceNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as InvoiceNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Order:
                    nft = new OrderNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as OrderNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Product:
                    nft = new ProductNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as ProductNFT).LoadLastData(meta);
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
                    nft.ParsePrice(meta);
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
                    nft.ParsePrice(meta);
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
                case NFTTypes.Device:
                    nft = new DeviceNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as DeviceNFT).LoadLastData(meta);
                    else
                        (nft as DeviceNFT).LoadLastData(meta);
                    break;
                case NFTTypes.IoTDevice:
                    nft = new IoTDeviceNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as IoTDeviceNFT).LoadLastData(meta);
                    else
                        (nft as IoTDeviceNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Protocol:
                    nft = new ProtocolNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as ProtocolNFT).LoadLastData(meta);
                    else
                        (nft as ProtocolNFT).LoadLastData(meta);
                    break;
                case NFTTypes.HWSrc:
                    nft = new HWSrcNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as HWSrcNFT).LoadLastData(meta);
                    else
                        (nft as HWSrcNFT).LoadLastData(meta);
                    break;
                case NFTTypes.FWSrc:
                    nft = new FWSrcNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as FWSrcNFT).LoadLastData(meta);
                    else
                        (nft as FWSrcNFT).LoadLastData(meta);
                    break;
                case NFTTypes.SWSrc:
                    nft = new SWSrcNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as SWSrcNFT).LoadLastData(meta);
                    else
                        (nft as SWSrcNFT).LoadLastData(meta);
                    break;
                case NFTTypes.MechSrc:
                    nft = new MechSrcNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    if (wait)
                        await (nft as MechSrcNFT).LoadLastData(meta);
                    else
                        (nft as MechSrcNFT).LoadLastData(meta);
                    break;
                case NFTTypes.IoTMessage:
                    nft = new IoTMessageNFT(utxo);
                    nft.TokenId = tokid;
                    nft.Time = Time;
                    nft.TxDetails = txinfo;
                    nft.UtxoIndex = utxoindex;
                    await (nft as IoTMessageNFT).LoadLastData(meta);
                    break;
            }

            if (VEDLDataContext.AllowCache && tokid == NFTHelpers.TokenId && VEDLDataContext.NFTCache.Count < VEDLDataContext.MaxCachedItems)
            {
                if (nft.Type != NFTTypes.IoTDevice)
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
                case NFTTypes.Invoice:
                    nft = new InvoiceNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Order:
                    nft = new OrderNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Product:
                    nft = new ProductNFT(NFT.Utxo);
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
                case NFTTypes.Device:
                    nft = new DeviceNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.IoTDevice:
                    nft = new IoTDeviceNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.Protocol:
                    nft = new ProtocolNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.HWSrc:
                    nft = new HWSrcNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.FWSrc:
                    nft = new FWSrcNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.SWSrc:
                    nft = new SWSrcNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.MechSrc:
                    nft = new MechSrcNFT(NFT.Utxo);
                    await nft.Fill(NFT);
                    return nft;
                case NFTTypes.IoTMessage:
                    nft = new IoTMessageNFT(NFT.Utxo);
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

            if (type == NFTTypes.IoTDevice) return null;

            if (utxoindex == 1 && metadata.TryGetValue("ReceiptFromPaymentUtxo", out var rfp))
                if (!string.IsNullOrEmpty(rfp))
                    type = NFTTypes.Receipt;

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
                case NFTTypes.Invoice:
                    nft = new InvoiceNFT(utxo);
                    break;
                case NFTTypes.Order:
                    nft = new OrderNFT(utxo);
                    break;
                case NFTTypes.Product:
                    nft = new ProductNFT(utxo);
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
                case NFTTypes.Device:
                    nft = new DeviceNFT(utxo);
                    break;
                case NFTTypes.IoTDevice:
                    nft = new IoTDeviceNFT(utxo);
                    break;
                case NFTTypes.Protocol:
                    nft = new ProtocolNFT(utxo);
                    break;
                case NFTTypes.HWSrc:
                    nft = new HWSrcNFT(utxo);
                    break;
                case NFTTypes.FWSrc:
                    nft = new FWSrcNFT(utxo);
                    break;
                case NFTTypes.SWSrc:
                    nft = new SWSrcNFT(utxo);
                    break;
                case NFTTypes.MechSrc:
                    nft = new MechSrcNFT(utxo);
                    break;
                case NFTTypes.IoTMessage:
                    nft = new IoTMessageNFT(utxo);
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
                if (nft.Type == NFTTypes.Message)
                    (nft as MessageNFT).Decrypted = false;
                if (nft.Type == NFTTypes.IoTMessage)
                    (nft as IoTMessageNFT).Decrypted = false;
                if (nft.Type == NFTTypes.IoTDevice)
                    (nft as IoTDeviceNFT).DecryptedSetting = false;

                return nft;
            }
            else
            {
                return null;
            }
        }
    }
}
