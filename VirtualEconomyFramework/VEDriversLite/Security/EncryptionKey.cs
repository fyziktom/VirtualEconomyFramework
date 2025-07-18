﻿using NBitcoin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.DogeAPI;

namespace VEDriversLite.Security
{
    /// <summary>
    /// Key container for the store of the key
    /// </summary>
    public class EncryptionKey
    {
        public EncryptionKey() { }
        /// <summary>
        /// Constructor to load the key
        /// </summary>
        /// <param name="key">key in raw form. If it is already encrypted please set fromDb flag</param>
        /// <param name="password">password to encrypt the key during the load to this container</param>
        /// <param name="fromDb">if this is set it will not encrypt the key with the provided password and just store key as it is. Pass must be loaded separately</param>
        public EncryptionKey(string key, string password = "", bool fromDb = false)
        {
            LoadNewKey(key, password, fromDb);
            //if (!string.IsNullOrEmpty(password))
            //    LoadPassword(password);

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }

        public EncryptionKey(string key, byte[] iv, string password = "", bool fromDb = false)
        {
            IV = iv;
            LoadNewKey(key, password, fromDb);
            //if (!string.IsNullOrEmpty(password))
            //    LoadPassword(password);

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }

        private string _key = string.Empty;

        /// <summary>
        /// ID of this key
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;
        /// <summary>
        /// Related item ID
        /// </summary>
        public Guid RelatedItemId { get; set; } = Guid.Empty;
        /// <summary>
        /// Name of the key
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Public key if there is some
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// IV
        /// </summary>
        public byte[] IV { get; set; } = null;
        /// <summary>
        /// Password hash
        /// </summary>
        public byte[] PasswordHash { get => Encoding.UTF8.GetBytes(loadedPassHash); }
        public string PasswordHashString { get => loadedPassHash; }

        private string loadedPassword = string.Empty;
        private string loadedPassHash = string.Empty;
        private bool passwordLoaded = false;
        /// <summary>
        /// Is the key encrypted flag
        /// </summary>
        public bool IsEncrypted { get; set; } = false;
        /// <summary>
        /// Version of the key
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// was key deleted
        /// </summary>
        public bool Deleted { get; set; }
        /// <summary>
        /// Is key loaded? Returns true if the key is not empty or null
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return !string.IsNullOrEmpty(_key);
            }
        }

        /// <summary>
        /// Is pass loaded? Returns true if the pass is loaded
        /// </summary>
        public bool IsPassLoaded
        {
            get
            {
                return passwordLoaded;
            }
        }

        /// <summary>
        /// This is confusing, sorry will be renamed soon
        /// It returns decrypted key if the pass is loaded. 
        /// If the pass is not loaded you need to provide pass
        /// If you want encrypted form of the key select returnEncrypted=true
        /// </summary>
        /// <param name="password">fill if the pass is not loaded and you need decrypted key</param>
        /// <param name="returnEncrypted">set true for return enrcypted form of key</param>
        /// <returns></returns>

        public async Task<string> GetEncryptedKey(string password = "", bool returnEncrypted = false)
        {
            if (returnEncrypted)
            {
                return SymetricProvider.JoinIVToString(_key, IV);
            }

            if (passwordLoaded && string.IsNullOrEmpty(password))
            {
                password = loadedPassword;
                loadedPassHash = SecurityUtils.ComputeSha256Hash(loadedPassword);
            }

            if (!passwordLoaded && string.IsNullOrEmpty(password))
                return null;

            if (!string.IsNullOrEmpty(password))
            {
                var done = false;
                try
                {
                    return await SymetricProvider.DecryptStringAsync(loadedPassHash, _key, IV);
                }
                catch (Exception e) {
                    ; 
                }
                if (!done)
                {
                    return await SymetricProvider.DecryptStringAsync(password, _key, IV);
                }
            }
            else
            {
                if (!IsEncrypted)
                    return SymetricProvider.JoinIVToString(_key, IV);
            }
            return null;
        }

        /// <summary>
        /// Load the key and password.
        /// Same logic as constructor of this class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="password"></param>
        /// <param name="fromDb"></param>
        /// <returns></returns>

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
                loadedPassHash = SecurityUtils.ComputeSha256Hash(loadedPassword);   
                passwordLoaded = true;

                _key = await SymetricProvider.EncryptStringAsync(loadedPassHash, key, IV);
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

        /// <summary>
        /// Lock the account. It will remove the loaded password and set passwordloaded flag to false
        /// </summary>
        public void Lock()
        {
            loadedPassword = string.Empty;
            loadedPassHash = string.Empty;
            passwordLoaded = false;
        }

        /// <summary>
        /// Load the password to the container
        /// It will set passwordLoaded flag to true
        /// </summary>
        /// <param name="password"></param>
        public void LoadPassword(string password)
        {            
            loadedPassword = password;
            loadedPassHash = SecurityUtils.ComputeSha256Hash(loadedPassword);
            passwordLoaded = true;
            if (!string.IsNullOrEmpty(password))
                IsEncrypted = true;
        }
    }
}
