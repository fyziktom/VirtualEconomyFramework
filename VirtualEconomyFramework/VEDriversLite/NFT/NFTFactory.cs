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
        public static async Task<INFT> GetNFT(string tokenId, 
                                              string utxo, 
                                              int utxoindex = 0, 
                                              double time = 0, 
                                              bool wait = false, 
                                              bool loadJustType = false, 
                                              NFTTypes justType = NFTTypes.Image,
                                              bool skipTheType = false,
                                              NFTTypes skipType = NFTTypes.Image)
        {
            NFTTypes type = NFTTypes.Image;

            var txinfo = await NeblioTransactionHelpers.GetTransactionInfo(utxo);
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

            var meta = await NeblioTransactionHelpers.GetTransactionMetadata(tokid, utxo);
            
            if (meta == null)
                return null;
            else if (meta.Count == 0 || meta.Count == 1)
                return null;

            if (meta.TryGetValue("Type", out var t))
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
                }
                else
                {
                    return null;
                }
            }
            else
            {
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

            INFT nft = null;
            switch (type)
            {
                case NFTTypes.Image:
                    nft = new ImageNFT(utxo);
                    nft.TxDetails = txinfo;
                    if (wait)
                        await nft.ParseOriginData(meta);
                    else
                        nft.ParseOriginData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
                case NFTTypes.Profile:
                    nft = new ProfileNFT(utxo);
                    nft.TxDetails = txinfo;
                    //await pnft.ParseOriginData();
                    if (wait)
                        await (nft as ProfileNFT).LoadLastData(meta);
                    else
                        (nft as ProfileNFT).LoadLastData(meta);
                    nft.Time = Time;
                    return nft;
                case NFTTypes.Post:
                    nft = new PostNFT(utxo);
                    nft.TxDetails = txinfo;
                    //await ponft.ParseOriginData();
                    if (wait)
                        await (nft as PostNFT).LoadLastData(meta);
                    else
                        (nft as PostNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Music:
                    nft = new MusicNFT(utxo);
                    nft.TxDetails = txinfo;
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
                    nft.TxDetails = txinfo;
                    if (wait)
                        await (nft as PaymentNFT).LoadLastData(meta);
                    else
                        (nft as PaymentNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Receipt:
                    nft = new ReceiptNFT(utxo);
                    nft.TxDetails = txinfo;
                    if (wait)
                        await (nft as ReceiptNFT).LoadLastData(meta);
                    else
                        (nft as ReceiptNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Message:
                    nft = new MessageNFT(utxo);
                    nft.TxDetails = txinfo;
                    if (wait)
                        await (nft as MessageNFT).LoadLastData(meta);
                    else
                        (nft as MessageNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Ticket:
                    nft = new TicketNFT(utxo);
                    nft.TxDetails = txinfo;
                    if (wait)
                        await nft.ParseOriginData(meta);
                    else
                        nft.ParseOriginData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
                case NFTTypes.Event:
                    nft = new EventNFT(utxo);
                    nft.TxDetails = txinfo;
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
                    nft.TxDetails = txinfo;
                    if (wait)
                        await (nft as CoruzantArticleNFT).LoadLastData(meta);
                    else
                        (nft as CoruzantArticleNFT).LoadLastData(meta);
                    break;
                case NFTTypes.CoruzantProfile:
                    nft = new CoruzantProfileNFT(utxo);
                    nft.TxDetails = txinfo;
                    if (wait)
                        await (nft as CoruzantProfileNFT).LoadLastData(meta);
                    else
                        (nft as CoruzantProfileNFT).LoadLastData(meta);
                    break;
            }

            nft.TokenId = tokid;
            nft.Time = Time;
            nft.TxDetails = txinfo;
            nft.UtxoIndex = utxoindex;

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
    }
}
