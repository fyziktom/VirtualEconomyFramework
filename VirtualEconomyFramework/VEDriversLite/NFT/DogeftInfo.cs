﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Info for Dogeft shop with NFTs
    /// </summary>
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
        /// Reward scheme for the sale of the NFT. Default is Dogeft standard : 80% to the author, 10% charity, 10% dogeft
        /// </summary>
        public string RewardSchemeName { get; set; } = string.Empty;
        /// <summary>
        /// Author Deposit Doge address where Dogeft will send the reward from sale
        /// </summary>
        public string AuthorDogeAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// If some of the required NFT info is empty retunrs false
        /// </summary>
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
