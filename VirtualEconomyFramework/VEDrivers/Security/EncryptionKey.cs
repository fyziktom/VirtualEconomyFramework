using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Database.Models;

namespace VEDrivers.Security
{
    public enum EncryptionKeyType
    {
        BasicSecurity,
        AccountKey,
        LicenseKey
    }
    public class EncryptionKey
    {
        public EncryptionKey(string key, string password = "")
        {
            LoadNewKey(key, password);
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
        public byte[] PasswordHash { get; set; }
        private string loadedPassword = string.Empty;
        private bool passwordLoaded = false;
        public bool IsEncrypted { get; set; } = false;
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
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
            passwordLoaded = true;
            return true;
        }
    }
}
