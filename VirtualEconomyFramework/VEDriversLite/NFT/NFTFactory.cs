using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public static class NFTFactory
    {
        public static async Task<INFT> GetNFT(NFTTypes type, string utxo)
        {
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
                    await pnft.ParseOriginData();
                    //await pnft.GetLastData();
                    if (!string.IsNullOrEmpty(pnft.NFTOriginTxId))
                        return pnft;
                    else
                        return null;
                case NFTTypes.Post:
                    var ponft = new PostNFT(utxo);
                    await ponft.ParseOriginData();
                    //await ponft.GetLastData();
                    if (!string.IsNullOrEmpty(ponft.NFTOriginTxId))
                        return ponft;
                    else
                        return null;
            }

            return null;
        }
    }
}
