using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Bookmarks
{
    public static class BookmarkFactory
    {
        public static IBookmark GetBookmark(BookmarkTypes type, Guid accId, string name, string address)
        {
            switch (type)
            {
                case BookmarkTypes.ShopBookmark:
                    return new ShopBookmark(accId, name, address);
                case BookmarkTypes.MessagingBookmark:
                    return new MessagingBookmark(accId, name, address);
            }

            return null;
        }
    }
}
