using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public static class NFTFactory
    {
        public static async Task<INFT> GetNFT(string tokenId, string utxo)
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
                    }
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

            switch (type)
            {
                case NFTTypes.Image:
                    var nft = new ImageNFT(utxo);
                    await nft.ParseOriginData();
                    if (!string.IsNullOrEmpty(nft.NFTOriginTxId))
                        return nft;
                    else
                        return null;
                case NFTTypes.Profile:
                    var pnft = new ProfileNFT(utxo);
                    //await pnft.ParseOriginData();
                    await pnft.LoadLastData(meta);
                    return pnft;
                case NFTTypes.Post:
                    var ponft = new PostNFT(utxo);
                    //await ponft.ParseOriginData();
                    await ponft.LoadLastData(meta);
                    return ponft;
            }

            return null;
        }
    }
}
