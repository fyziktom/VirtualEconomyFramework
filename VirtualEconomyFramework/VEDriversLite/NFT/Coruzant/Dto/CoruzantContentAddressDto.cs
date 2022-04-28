using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Coruzant.Dto
{
    /// <summary>
    /// Dto for the coruzant address
    /// </summary>
    public class CoruzantContentAddressDto
    {
        /// <summary>
        /// Address with content
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Name of the page - block of the content
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Short description of the content
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Image link of this block of the content
        /// </summary>
        public string ImageLink { get; set; } = string.Empty;
        /// <summary>
        /// Main profile NFT link/hash
        /// </summary>
        public string ProfileLink { get; set; } = string.Empty;
        /// <summary>
        /// List of the tags describing this content
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

    }
}
