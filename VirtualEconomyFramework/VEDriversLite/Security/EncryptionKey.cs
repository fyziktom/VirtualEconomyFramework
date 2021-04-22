using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Security
{
    public enum EncryptionKeyType
    {
        BasicSecurity,
        AccountKey,
        LicenseKey
    }
    public class EncryptionKeyTransferDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public EncryptionKeyType Type { get; set; } = EncryptionKeyType.BasicSecurity;
        public string PublicKey { get; set; } = string.Empty;
        public string EncryptedKey { get; set; } = string.Empty;
        public string RelatedItemId { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; } = false;

        public void Update(EncryptionKey key)
        {
            Id = key.Id.ToString();
            Name = key.Name;
            PublicKey = key.PublicKey;
            EncryptedKey = key.GetEncryptedKey(returnEncrypted: true);
            RelatedItemId = key.RelatedItemId.ToString();
            PasswordHash = Encoding.UTF8.GetString(key.PasswordHash);
            IsEncrypted = key.IsEncrypted;
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
                key.Id = new Guid();
            }

            try
            {
                key.RelatedItemId = new Guid(RelatedItemId);
            }
            catch (Exception ex)
            {
                key.RelatedItemId = new Guid();
            }

            key.IsEncrypted = (bool)IsEncrypted;
            key.Name = Name;
            key.PublicKey = PublicKey;
            key.Type = (EncryptionKeyType)Type;

            if (PasswordHash?.Length > 0)
                key.PasswordHash = Encoding.UTF8.GetBytes(PasswordHash);

            if (!string.IsNullOrEmpty(EncryptedKey))
                key.LoadNewKey(EncryptedKey, fromDb: true);

            return key;
        }
    }
    public class EncryptionKey
    {
        public EncryptionKey(string key, string password = "", bool fromDb = false)
        {
            LoadNewKey(key, password, fromDb);
            if (!string.IsNullOrEmpty(password))
                LoadPassword(password);

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }


        private string _key = string.Empty;

        public EncryptionKeyType Type { get; set; } = EncryptionKeyType.BasicSecurity;
        public Guid Id { get; set; } = Guid.Empty;
        public Guid RelatedItemId { get; set; } = Guid.Empty;
        public string Name { get; set; }
        public string PublicKey { get; set; }
        public byte[] PasswordHash { get; set; }
        private string loadedPassword = string.Empty;
        private bool passwordLoaded = false;
        public bool IsEncrypted { get; set; } = false;
        public string Version { get; set; }
        public bool Deleted { get; set; }
        public bool IsLoaded
        {
            get
            {
                return !string.IsNullOrEmpty(_key);
            }
        }

        public bool IsPassLoaded
        {
            get
            {
                return passwordLoaded;
            }
        }

        public string GetEncryptedKey(string password = "", bool returnEncrypted = false)
        {
            if (returnEncrypted)
            {
                return _key;
            }

            if (passwordLoaded && string.IsNullOrEmpty(password))
                password = loadedPassword;

            if (!passwordLoaded && string.IsNullOrEmpty(password))
                return null;

            if (!string.IsNullOrEmpty(password))
            {
                if (PasswordHash?.Length > 0)
                {
                    if (SecurityUtil.VerifyPassword(password, PasswordHash))
                        return SymetricProvider.DecryptString(password, _key);
                    else
                        return null;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (!IsEncrypted)
                    return _key;
                else
                    return null;
            }
        }

        public bool LoadNewKey(string key, string password = "", bool fromDb = false)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (fromDb)
            {
                _key = key;
                return true;
            }

            if (!string.IsNullOrEmpty(password))
            {
                loadedPassword = password;
                passwordLoaded = true;
                PasswordHash = SecurityUtil.HashPassword(password);
                _key = SymetricProvider.EncryptString(password, key);
                IsEncrypted = true;
                return true;
            }
            else
            {
                _key = key;
                IsEncrypted = false;
                return true;
            }
        }

        public void Lock()
        {
            loadedPassword = string.Empty;
            passwordLoaded = false;
        }

        public bool LoadPassword(string password)
        {
            loadedPassword = password;
            PasswordHash = SecurityUtil.HashPassword(password);
            passwordLoaded = true;
            return true;
        }
    }
}
