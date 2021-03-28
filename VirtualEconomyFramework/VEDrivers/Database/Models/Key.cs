using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;
using VEDrivers.Security;

namespace VEDrivers.Database.Models
{
    public class Key : Entity
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int Type { get; set; }
        public string RelatedItemId { get; set; }
        public string StoredKey { get; set; }
        public byte[] PasswordHash { get; set; }
        public bool? IsEncrypted { get; set; }

        public void Update(EncryptionKey key)
        {
            if (string.IsNullOrEmpty(Id))
            {
                if (!string.IsNullOrEmpty(key.Id.ToString()))
                {
                    Id = key.Id.ToString();
                }
                else
                {
                    Id = Guid.NewGuid().ToString();
                }
            }

            IsEncrypted = key.IsEncrypted;
            StoredKey = key.GetEncryptedKey(returnEncrypted : true);

            if (key.PasswordHash?.Length > 0)
                PasswordHash = key.PasswordHash;
            else
                PasswordHash = null;

            Name = key.Name;
            if (key.RelatedItemId != Guid.Empty)
                RelatedItemId = key.RelatedItemId.ToString();

            Type = (int)key.Type;

            if (!string.IsNullOrEmpty(key.Version))
                Version = key.Version;
            else
                Version = "0.1";
            ModifiedOn = DateTime.UtcNow;

            Deleted = (bool)key.Deleted;

            if (string.IsNullOrEmpty(ModifiedBy) && !string.IsNullOrEmpty(key.CreatedBy))
                ModifiedBy = key.ModifiedBy;
            else if (string.IsNullOrEmpty(ModifiedBy) && string.IsNullOrEmpty(key.ModifiedBy))
                ModifiedBy = "admin";

            if (string.IsNullOrEmpty(CreatedBy) && !string.IsNullOrEmpty(key.CreatedBy))
                CreatedBy = key.CreatedBy;
            else if (string.IsNullOrEmpty(CreatedBy) && string.IsNullOrEmpty(key.CreatedBy))
                CreatedBy = "admin";

            CreatedOn = key.CreatedOn;
        }

        public EncryptionKey Fill(EncryptionKey key)
        {
            if (key == null)
                return null;

            try
            {
                key.Id = new Guid(Id);
            }
            catch (Exception ex)
            {
                log.Error("Node Guid stored in db is not valid, creating new one", ex);
                key.Id = new Guid();
            }

            try
            {
                key.RelatedItemId = new Guid(RelatedItemId);
            }
            catch (Exception ex)
            {
                log.Error("RelatedItemId Guid stored in db is not valid, creating new one", ex);
                key.RelatedItemId = new Guid();
            }

            key.IsEncrypted = (bool)IsEncrypted;
            key.Name = Name;
            key.Type = (EncryptionKeyType)Type;
            key.Version = Version;
            key.ModifiedOn = ModifiedOn;
            key.ModifiedBy = ModifiedBy;
            key.Deleted = Deleted;
            key.CreatedBy = CreatedBy;
            key.CreatedOn = CreatedOn;

            if (PasswordHash?.Length > 0)
                key.PasswordHash = PasswordHash;

            if (!string.IsNullOrEmpty(StoredKey))
                key.LoadNewKey(StoredKey, fromDb: true);

            return key;
        }

    }
}
