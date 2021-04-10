using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Bookmarks;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;
using VEDrivers.Security;

namespace VEDrivers.Database.Models
{
    public class BookmarkEntity : Entity
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int Type { get; set; }
        public string Address { get; set; }
        public string RelatedItemId { get; set; }

        public void Update(IBookmark bookmark)
        {
            if (string.IsNullOrEmpty(Id))
            {
                if (!string.IsNullOrEmpty(bookmark.Id.ToString()))
                {
                    Id = bookmark.Id.ToString();
                }
                else
                {
                    Id = Guid.NewGuid().ToString();
                }
            }

            if (!string.IsNullOrEmpty(bookmark.Address))
                Address = bookmark.Address;

            Name = bookmark.Name;
            if (bookmark.RelatedItemId != Guid.Empty)
                RelatedItemId = bookmark.RelatedItemId.ToString();

            Type = (int)bookmark.Type;

            if (!string.IsNullOrEmpty(bookmark.Version))
                Version = bookmark.Version;
            else
                Version = "0.1";
            ModifiedOn = DateTime.UtcNow;

            Deleted = (bool)bookmark.Deleted;

            if (string.IsNullOrEmpty(ModifiedBy) && !string.IsNullOrEmpty(bookmark.CreatedBy))
                ModifiedBy = bookmark.ModifiedBy;
            else if (string.IsNullOrEmpty(ModifiedBy) && string.IsNullOrEmpty(bookmark.ModifiedBy))
                ModifiedBy = "admin";

            if (string.IsNullOrEmpty(CreatedBy) && !string.IsNullOrEmpty(bookmark.CreatedBy))
                CreatedBy = bookmark.CreatedBy;
            else if (string.IsNullOrEmpty(CreatedBy) && string.IsNullOrEmpty(bookmark.CreatedBy))
                CreatedBy = "admin";

            CreatedOn = bookmark.CreatedOn;
        }

        public IBookmark Fill(IBookmark bookmark)
        {
            if (bookmark == null)
                return null;

            try
            {
                bookmark.Id = new Guid(Id);
            }
            catch (Exception ex)
            {
                log.Error("Node Guid stored in db is not valid, creating new one", ex);
                bookmark.Id = new Guid();
            }

            try
            {
                bookmark.RelatedItemId = new Guid(RelatedItemId);
            }
            catch (Exception ex)
            {
                log.Error("RelatedItemId Guid stored in db is not valid, creating new one", ex);
                bookmark.RelatedItemId = new Guid();
            }

            bookmark.Name = Name;
            bookmark.Type = (BookmarkTypes)Type;
            bookmark.Version = Version;
            bookmark.ModifiedOn = ModifiedOn;
            bookmark.ModifiedBy = ModifiedBy;
            bookmark.Deleted = Deleted;
            bookmark.CreatedBy = CreatedBy;
            bookmark.CreatedOn = CreatedOn;

            if (!string.IsNullOrEmpty(Address))
                bookmark.Address = Address;

            return bookmark;
        }

    }
}
