using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

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

            switch (type)
            {
                case NFTTypes.Image:
                    var nft = new ImageNFT(utxo);
                    if (wait)
                        await nft.ParseOriginData();
                    else
                        nft.ParseOriginData();
                    nft.Price = Price;
                    nft.PriceActive = PriceActive;
                    nft.Time = Time;
                    return nft;
                case NFTTypes.Profile:
                    var pnft = new ProfileNFT(utxo);
                    //await pnft.ParseOriginData();
                    if (wait)
                        await pnft.LoadLastData(meta);
                    else
                        pnft.LoadLastData(meta);
                    pnft.Time = Time;
                    return pnft;
                case NFTTypes.Post:
                    var ponft = new PostNFT(utxo);
                    //await ponft.ParseOriginData();
                    if (wait)
                        await ponft.LoadLastData(meta);
                    else
                        ponft.LoadLastData(meta);
                    ponft.Time = Time;
                    return ponft;
                case NFTTypes.Music:
                    var mnft = new MusicNFT(utxo);
                    //await ponft.ParseOriginData();
                    if (wait)
                        await mnft.ParseOriginData();
                    else
                        mnft.ParseOriginData();
                    mnft.Price = Price;
                    mnft.PriceActive = PriceActive;
                    mnft.Time = Time;
                    return mnft;
                case NFTTypes.Payment:
                    var pmnft = new PaymentNFT(utxo);
                    if (wait)
                        await pmnft.LoadLastData(meta);
                    else
                        pmnft.LoadLastData(meta);
                    pmnft.Time = Time;
                    return pmnft;
                case NFTTypes.Message:
                    var msgnft = new MessageNFT(utxo);
                    if (wait)
                        await msgnft.LoadLastData(meta);
                    else
                        msgnft.LoadLastData(meta);
                    msgnft.Time = Time;
                    return msgnft;
            }

            return null;
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
            }

            return null;
        }
    }
}
