using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Bookmarks
{
    /// <summary>
    /// Bookmark with saved blockchain Address, name and description
    /// </summary>
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
    }
}
