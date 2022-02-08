using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Security
{
    public class EncryptionKey
    {
        public EncryptionKey(string key, string password = "", bool fromDb = false)
        {
            LoadNewKey(key, password, fromDb);
            //if (!string.IsNullOrEmpty(password))
            //    LoadPassword(password);

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }

        private string _key = string.Empty;

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

        public async Task<string> GetEncryptedKey(string password = "", bool returnEncrypted = false)
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
                return await SymetricProvider.DecryptString(password, _key);
            }
            else
            {
                if (!IsEncrypted)
                    return _key;
                else
                    return null;
            }
        }

        public async Task<bool> LoadNewKey(string key, string password = "", bool fromDb = false)
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
                _key = await SymetricProvider.EncryptString(password, key);
                IsEncrypted = true;
                return true;
            }
            else
            {
                _key = key;
                IsEncrypted = false;
                if (!string.IsNullOrEmpty(password))
                    loadedPassword = string.Empty;
                else
                    loadedPassword = password;
                passwordLoaded = true;
                return true;
            }
        }

        public void Lock()
        {
            loadedPassword = string.Empty;
            passwordLoaded = false;
        }

        public void LoadPassword(string password)
        {            
            loadedPassword = password;
            passwordLoaded = true;
            if (!string.IsNullOrEmpty(password))
                IsEncrypted = true;
        }
    }
}
