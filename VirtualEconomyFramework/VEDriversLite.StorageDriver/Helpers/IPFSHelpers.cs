using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.Helpers
{
    public static class IPFSHelpers
    {
        /// <summary>
        /// Default Public IPFS Gateway address
        /// </summary>
        public static string GatewayURL = "https://ipfs.io/ipfs/";

        /// <summary>
        /// Remove the server address from link and return just IPFS Hash
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static string GetHashFromIPFSLink(string link)
        {
            if (string.IsNullOrEmpty(link)) return string.Empty;
            if (link.Contains("/ipfs/"))
            {
                var split = link.Split("/ipfs/");
                if (split != null && split.Length > 1)
                {
                    var hash = split[split.Length - 1] ?? string.Empty;
                    return hash;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// Get full IPFS link from the hash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string GetIPFSLinkFromHash(string? hash)
        {
            if (hash.Contains("http"))
                return hash;
            else
                return !string.IsNullOrEmpty(hash) ? string.Concat(GatewayURL, hash) : string.Empty;
        }
    }
}
