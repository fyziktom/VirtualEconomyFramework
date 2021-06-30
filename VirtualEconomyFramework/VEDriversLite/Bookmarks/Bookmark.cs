using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Bookmarks
{
    public class Bookmark
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsSubAccount { get; set; } = false;
    }
}
