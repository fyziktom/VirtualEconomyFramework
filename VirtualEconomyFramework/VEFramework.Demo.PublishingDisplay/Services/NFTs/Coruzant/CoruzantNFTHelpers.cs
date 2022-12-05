using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEFramework.Demo.PublishingDisplay.Services.NFTs.Coruzant
{
    /// <summary>
    /// Helper class to load and handle Coruzant NFTs
    /// Will be replaced with INFTModules soon
    /// </summary>
    public static class CoruzantNFTHelpers
    {
        /// <summary>
        /// Coruzant token ID - CORZT on Neblio Blockchain
        /// </summary>
        public static string CoruzantTokenId { get; set; } = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";

        /// <summary>
        /// Filter just Coruzant NFTs from NFT list
        /// </summary>
        /// <param name="allNFTs"></param>
        /// <returns></returns>
        public static async Task<List<INFT>> GetCoruzantNFTs(List<INFT> allNFTs)
        {
            if (allNFTs == null)
                return new List<INFT>();
            else
                return allNFTs.Where(n =>  n.Type == NFTTypes.CoruzantArticle ||
                                           n.Type == NFTTypes.CoruzantPodcast ||
                                           n.Type == NFTTypes.CoruzantPremiumArticle ||
                                           n.Type == NFTTypes.CoruzantPremiumPodcast ||
                                           n.Type == NFTTypes.CoruzantProfile).ToList();
        }

        /// <summary>
        /// This function will return first profile NFT in NFTs list.
        /// </summary>
        /// <param name="nfts"></param>
        /// <returns></returns>
        public static async Task<CoruzantProfileNFT> FindCoruzantProfileNFT(ICollection<INFT> nfts)
        {
            if (nfts != null)
                foreach (var n in nfts)
                    if (n.Type == NFTTypes.CoruzantProfile)
                        return (CoruzantProfileNFT)n;
            return new CoruzantProfileNFT("");
        }


    }
}
