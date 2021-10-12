using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Security
{
    public static class SecurityUtils
    {
        const int iterations = 10000;
        const int saltSize = 128 / 8;
        const int keySize = 256 / 8;

        /// <summary>
        /// Create hash of the password to store
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<byte[]> HashPassword(string password)
        {
            using (var algorithm = new Rfc2898DeriveBytes(password, saltSize, iterations))
            {
                var salt = algorithm.Salt;
                Array.Resize(ref salt, saltSize + keySize);
                Array.Copy(algorithm.GetBytes(keySize), 0, salt, saltSize, keySize);
                return salt;
            }
        }
        /// <summary>
        /// Verify the password against the hash of the password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static async Task<bool> VerifyPassword(string password, byte[] hash)
        {
            if (hash.Length != saltSize + keySize) return false;
            var salt = hash;
            Array.Resize(ref salt, saltSize);

            using (var algorithm = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                var key = algorithm.GetBytes(keySize);
                for (int i = 0; i < keySize; i++) if (hash[saltSize + i] != key[i]) return false;
                return true;
            }
        }

        /// <summary>
        /// Compute SHA256 Hash
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
