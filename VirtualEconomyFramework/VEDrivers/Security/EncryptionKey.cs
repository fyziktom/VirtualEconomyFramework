using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Security
{
    public class EncryptionKey
    {
        public EncryptionKey(string key, string password = "")
        {
            LoadNewKey(key, password);
        }

        private string _key = string.Empty;

        public bool IsEncrypted { get; set; } = false;
        public bool IsLoaded
        {
            get
            {
                return !string.IsNullOrEmpty(_key);
            }
        }

        public string GetEncryptedKey(string password = "")
        {
            if (!string.IsNullOrEmpty(password))
            {
                return SymetricProvider.DecryptString(password, _key);
            }
            else
            {
                if (!IsEncrypted)
                    return _key;
                else
                    return null;
            }
        }

        public bool LoadNewKey(string key, string password = "")
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (!string.IsNullOrEmpty(password))
            {
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
    }
}
