using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Tags
{
    /// <summary>
    /// Tag with rememered info about usage, etc.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Tag name
        /// </summary>        
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Command
        /// </summary>
        public string Command { get; set; } = string.Empty;
        /// <summary>
        /// Count of usage
        /// </summary>
        public int Count { get; set; } = 0;
        /// <summary>
        /// Last date of usage
        /// </summary>
        public DateTime LastUse { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Related tags hashes list
        /// </summary>
        public List<string> RelatedTags { get; set; } = new List<string>();
    }
}
