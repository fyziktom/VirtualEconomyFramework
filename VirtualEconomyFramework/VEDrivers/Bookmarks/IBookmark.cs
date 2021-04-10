using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;

namespace VEDrivers.Bookmarks
{
    public enum BookmarkTypes
    {
        ShopBookmark,
        MessagingBookmark
    }
    public interface IBookmark : ICommonDbObjectBase
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        Guid RelatedItemId { get; set; }
        BookmarkTypes Type { get; set; }
    }

}
