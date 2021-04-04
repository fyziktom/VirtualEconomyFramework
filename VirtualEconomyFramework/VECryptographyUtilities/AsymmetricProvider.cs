using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VECryptographyUtilities
{
    //https://riptutorial.com/csharp/example/32687/fast-asymmetric-file-encryption
    public static class AsymmetricProvider
    {
        #region Key Generation
        public class KeyPair
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }

        public static KeyPair GenerateNewKeyPair(int keySize = 4096)
        {
            // KeySize is measured in bits. 1024 is the default, 2048 is better, 4096 is more robust but takes a fair bit longer to generate.
            using (var rsa = new RSACryptoServiceProvider(keySize))
            {
                return new KeyPair { PublicKey = rsa.ToXmlString(false), PrivateKey = rsa.ToXmlString(true) };
            }
        }

        #endregion

        #region Asymmetric Data Encryption and Decryption

        public static byte[] EncryptData(byte[] data, string publicKey)
        {
            using (var asymmetricProvider = new RSACryptoServiceProvider())
            {
                asymmetricProvider.FromXmlString(publicKey);
                return asymmetricProvider.Encrypt(data, true);
            }
        }

        public static byte[] DecryptData(byte[] data, string publicKey)
        {
            using (var asymmetricProvider = new RSACryptoServiceProvider())
            {
                asymmetricProvider.FromXmlString(publicKey);
                //if (asymmetricProvider.PublicOnly)
                //throw new Exception("The key provided is a public key and does not contain the private key elements required for decryption");
                return asymmetricProvider.Decrypt(data, true);
            }
        }

        public static string EncryptString(string value, string publicKey)
        {
            return Convert.ToBase64String(EncryptData(Encoding.UTF8.GetBytes(value), publicKey));
        }

        public static string DecryptString(string value, string privateKey)
        {
            return Encoding.UTF8.GetString(DecryptData(Convert.FromBase64String(value), privateKey));
        }

        #endregion

        #region Hybrid File Encryption and Decription

        public static void EncryptFile(string inputFilePath, string outputFilePath, string publicKey)
        {
            using (var symmetricCypher = new AesManaged())
            {
                // Generate random key and IV for symmetric encryption
                var key = new byte[symmetricCypher.KeySize / 8];
                var iv = new byte[symmetricCypher.BlockSize / 8];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(key);
                    rng.GetBytes(iv);
                }

                // Encrypt the symmetric key and IV
                var buf = new byte[key.Length + iv.Length];
                Array.Copy(key, buf, key.Length);
                Array.Copy(iv, 0, buf, key.Length, iv.Length);
                buf = EncryptData(buf, publicKey);

                var bufLen = BitConverter.GetBytes(buf.Length);

                // Symmetrically encrypt the data and write it to the file, along with the encrypted key and iv
                using (var cypherKey = symmetricCypher.CreateEncryptor(key, iv))
                using (var fsIn = new FileStream(inputFilePath, FileMode.Open))
                using (var fsOut = new FileStream(outputFilePath, FileMode.Create))
                using (var cs = new CryptoStream(fsOut, cypherKey, CryptoStreamMode.Write))
                {
                    fsOut.Write(bufLen, 0, bufLen.Length);
                    fsOut.Write(buf, 0, buf.Length);
                    fsIn.CopyTo(cs);
                }
            }
        }

        public static void DecryptFile(string inputFilePath, string outputFilePath, string privateKey)
        {
            using (var symmetricCypher = new AesManaged())
            using (var fsIn = new FileStream(inputFilePath, FileMode.Open))
            {
                // Determine the length of the encrypted key and IV
                var buf = new byte[sizeof(int)];
                fsIn.Read(buf, 0, buf.Length);
                var bufLen = BitConverter.ToInt32(buf, 0);

                // Read the encrypted key and IV data from the file and decrypt using the asymmetric algorithm
                buf = new byte[bufLen];
                fsIn.Read(buf, 0, buf.Length);
                buf = DecryptData(buf, privateKey);

                var key = new byte[symmetricCypher.KeySize / 8];
                var iv = new byte[symmetricCypher.BlockSize / 8];
                Array.Copy(buf, key, key.Length);
                Array.Copy(buf, key.Length, iv, 0, iv.Length);

                // Decript the file data using the symmetric algorithm
                using (var cypherKey = symmetricCypher.CreateDecryptor(key, iv))
                using (var fsOut = new FileStream(outputFilePath, FileMode.Create))
                using (var cs = new CryptoStream(fsOut, cypherKey, CryptoStreamMode.Write))
                {
                    fsIn.CopyTo(cs);
                }
            }
        }

        #endregion

        #region Key Storage

        public static void WritePublicKey(string publicKeyFilePath, string publicKey)
        {
            File.WriteAllText(publicKeyFilePath, publicKey);
        }
        public static string ReadPublicKey(string publicKeyFilePath)
        {
            return File.ReadAllText(publicKeyFilePath);
        }

        private const string SymmetricSalt = "Stack_Overflow!"; // Change me!

        public static string ReadPrivateKey(string privateKeyFilePath, string password)
        {
            var salt = Encoding.UTF8.GetBytes(SymmetricSalt);
            var cypherText = File.ReadAllBytes(privateKeyFilePath);

            using (var cypher = new AesManaged())
            {
                var pdb = new Rfc2898DeriveBytes(password, salt);
                var key = pdb.GetBytes(cypher.KeySize / 8);
                var iv = pdb.GetBytes(cypher.BlockSize / 8);

                using (var decryptor = cypher.CreateDecryptor(key, iv))
                using (var msDecrypt = new MemoryStream(cypherText))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        public static void WritePrivateKey(string privateKeyFilePath, string privateKey, string password)
        {
            var salt = Encoding.UTF8.GetBytes(SymmetricSalt);
            using (var cypher = new AesManaged())
            {
                var pdb = new Rfc2898DeriveBytes(password, salt);
                var key = pdb.GetBytes(cypher.KeySize / 8);
                var iv = pdb.GetBytes(cypher.BlockSize / 8);

                using (var encryptor = cypher.CreateEncryptor(key, iv))
                using (var fsEncrypt = new FileStream(privateKeyFilePath, FileMode.Create))
                using (var csEncrypt = new CryptoStream(fsEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(privateKey);
                }
            }
        }

        #endregion
    }
}
