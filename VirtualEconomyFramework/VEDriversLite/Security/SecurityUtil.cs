using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Security
{
    public static class SecurityUtil
    {
        const int iterations = 10000;
        const int saltSize = 128 / 8;
        const int keySize = 256 / 8;

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
    }
}
