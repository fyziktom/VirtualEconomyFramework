using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace VEFramework.Demo.MusicBandDisplay.Services.NFTs
{
    /// <summary>
    /// NFT shared data context
    /// </summary>
    public static class NFTDataContext
    {
        /// <summary>
        /// All loaded apps in instance
        /// </summary>     
        public static ConcurrentDictionary<string, Tags.Tag> Tags { get; set; } = new ConcurrentDictionary<string, Tags.Tag>();
    }
}
