using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT
{
    public class DogeftInfo
    {
        /// <summary>
        /// Here can be described license
        /// </summary>
        public string License { get; set; } = string.Empty;
        /// <summary>
        /// If this is 0 (default) it is unique NFT
        /// </summary>
        public int Coppies { get; set; } = 0;
        /// <summary>
        /// If you want to provide some author personal page
        /// </summary>
        public string AuthorUrl { get; set; } = string.Empty;
        /// <summary>
        /// Author Deposit Doge address where Dogeft will send the reward from sale
        /// </summary>
        public string AuthorDogeAddress { get; set; } = string.Empty;
        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                var res = true;
                if (!string.IsNullOrEmpty(AuthorDogeAddress))
                    res = false;
                if (!string.IsNullOrEmpty(License))
                    res = false;
                if (!string.IsNullOrEmpty(AuthorUrl))
                    res = false;
                if (Coppies > 0)
                    res = false;

                return res;
            }
        }

    }
}
