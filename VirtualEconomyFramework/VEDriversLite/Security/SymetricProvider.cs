using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Security
{
    //https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
    public static class SymetricProvider
    {
        /// <summary>
        /// Symmetrical encryption of the text.
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="plainText">Text which should be encrypted</param>
        /// <returns></returns>
        public static async Task<string> EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                var keySize = 256 / 8;

                var k = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref k, keySize);
                aes.Key = k; // todo wrong lenght!!
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        /// <summary>
        /// Symmetrical encryption of the bytes
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="bytes">Bytes array which should be encrypted</param>
        /// <returns></returns>
        public static async Task<byte[]> EncryptBytes(string key, byte[] bytes)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                var keySize = 256 / 8;

                var k = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref k, keySize);
                aes.Key = k; // todo wrong lenght!!
                aes.IV = iv;


                using (MemoryStream stream = new MemoryStream())
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (CryptoStream encrypt = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                {
                    encrypt.Write(bytes, 0, bytes.Length);
                    encrypt.FlushFinalBlock();
                    return stream.ToArray();
                }
            }
        }
        /// <summary>
        /// Symmetrical decryption of the text
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="cipherText">Text which should be decrypted</param>
        /// <returns></returns>
        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                var keySize = 256 / 8;

                var k = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref k, keySize);
                aes.Key = k; // todo wrong lenght!!

                //aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Symmetrical decryption of the bytes
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="bytes">Bytes array which should be decrypted</param>
        /// <returns></returns>
        public static async Task<byte[]> DecryptBytes(string key, byte[] cipherBytes)
        {
            byte[] iv = new byte[16];
            byte[] buffer = cipherBytes;//Convert.FromBase64String(cipherBytes);

            using (Aes aes = Aes.Create())
            {
                var keySize = 256 / 8;

                var k = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref k, keySize);
                aes.Key = k; // todo wrong lenght!!

                //aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                using (MemoryStream stream = new MemoryStream())
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (CryptoStream encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
                {
                    encrypt.Write(cipherBytes, 0, cipherBytes.Length);
                    encrypt.FlushFinalBlock();
                    return stream.ToArray();
                }
            }
        }
    }
}
