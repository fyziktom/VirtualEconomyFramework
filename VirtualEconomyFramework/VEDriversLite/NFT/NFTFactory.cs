using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT.Coruzant;

namespace VEDriversLite.NFT
{
    public static class NFTFactory
    {
        public static async Task<INFT> GetNFT(string tokenId, string utxo, double time = 0, bool wait = false)
        {
            NFTTypes type = NFTTypes.Image;

            var meta = await NeblioTransactionHelpers.GetTransactionMetadata(tokenId, utxo);
            
            if (meta == null)
            {
                return null;
            }

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
                        case "NFT CoruzantPost":
                            type = NFTTypes.CoruzantPost;
                            break;
                        case "NFT CoruzantPremiumPost":
                            type = NFTTypes.CoruzantPremiumPost;
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
                    if (wait)
                        await nft.ParseOriginData(meta);
                    else
                        nft.ParseOriginData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
                case NFTTypes.Profile:
                    nft = new ProfileNFT(utxo);
                    //await pnft.ParseOriginData();
                    if (wait)
                        await (nft as ProfileNFT).LoadLastData(meta);
                    else
                        (nft as ProfileNFT).LoadLastData(meta);
                    nft.Time = Time;
                    return nft;
                case NFTTypes.Post:
                    nft = new PostNFT(utxo);
                    //await ponft.ParseOriginData();
                    if (wait)
                        await (nft as PostNFT).LoadLastData(meta);
                    else
                        (nft as PostNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Music:
                    nft = new MusicNFT(utxo);
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
                    if (wait)
                        await (nft as PaymentNFT).LoadLastData(meta);
                    else
                        (nft as PaymentNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Message:
                    nft = new MessageNFT(utxo);
                    if (wait)
                        await (nft as MessageNFT).LoadLastData(meta);
                    else
                        (nft as MessageNFT).LoadLastData(meta);
                    break;
                case NFTTypes.Ticket:
                    nft = new TicketNFT(utxo);
                    //if (wait)
                        await nft.ParseOriginData(meta);
                    //else
                    //    nft.ParseOriginData(meta);
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    break;
                case NFTTypes.CoruzantPost:
                    nft = new CoruzantPostNFT(utxo);
                    if (wait)
                        await (nft as CoruzantPostNFT).LoadLastData(meta);
                    else
                        (nft as CoruzantPostNFT).LoadLastData(meta);
                    break;
                case NFTTypes.CoruzantProfile:
                    nft = new CoruzantProfileNFT(utxo);
                    if (wait)
                        await (nft as CoruzantProfileNFT).LoadLastData(meta);
                    else
                        (nft as CoruzantProfileNFT).LoadLastData(meta);
                    break;
            }

            nft.TokenId = tokenId;
            nft.Time = Time;

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
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.Profile:
                    nft = new ProfileNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.Post:
                    nft = new PostNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.Music:
                    nft = new MusicNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.Payment:
                    nft = new PaymentNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.Message:
                    nft = new MessageNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.Ticket:
                    nft = new TicketNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.CoruzantPost:
                    nft = new CoruzantPostNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
                case NFTTypes.CoruzantProfile:
                    nft = new CoruzantProfileNFT(NFT.Utxo);
                    nft.Fill(NFT);
                    return nft;
            }

            return null;
        }
    }
}
