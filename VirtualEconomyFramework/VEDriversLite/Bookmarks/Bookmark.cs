using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Bookmarks
{
    public class Bookmark
    {
        /// <summary>
        /// Name of the bookmark
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Blockchain address of the Bookmark
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Note about bookmark
        /// </summary>
        public string Note { get; set; } = string.Empty;
        /// <summary>
        /// If this is true, the bookmark is on some subaccount
        /// </summary>
        public bool IsSubAccount { get; set; } = false;
        /// <summary>
        /// Related Address where this Bookmark is saved to, if any
        /// </summary>
        public string RelatedAddress { get; set; } = string.Empty;
    }
}
