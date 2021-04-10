using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Bookmarks
{
    public class ShopBookmark : CommonBookmark
    {
        public ShopBookmark(Guid relatedItemId, string name, string address)
        {
            RelatedItemId = relatedItemId;
            Name = name;
            Address = address;
        }
    }
}
