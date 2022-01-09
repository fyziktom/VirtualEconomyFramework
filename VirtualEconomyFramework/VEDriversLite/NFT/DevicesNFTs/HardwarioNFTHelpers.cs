using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Security;

namespace VEDriversLite.NFT.DevicesNFTs
{
    public static class HardwarioNFTHelpers
    {
        public static string TokenId { get; set; } = "La2kHrELu2RokMELtgcZrrXL6bqC6QaU8W3TUb";

        /// <summary>
        /// Filter just Coruzant NFTs from NFT list
        /// </summary>
        /// <param name="allNFTs"></param>
        /// <returns></returns>
        public static async Task<List<INFT>> GetHARDWARIONFTs(List<INFT> allNFTs)
        {
            if (allNFTs == null)
                return new List<INFT>();
            else
                return allNFTs.Where(n => n.TokenId == TokenId).ToList();
        }

    }
}
