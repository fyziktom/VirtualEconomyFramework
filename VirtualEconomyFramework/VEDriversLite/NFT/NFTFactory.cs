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
            }

            return null;
        }
    }
}
