using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace VEDriversLite.Security
{

    public static class SymetricProvider
    {
        private static byte[] GetKeyBytes(string key)
        {
            var keybytes = Encoding.UTF8.GetBytes(key);

            var keySize = 256 / 8;
            Array.Resize(ref keybytes, keySize);
            return keybytes;
        }

        #region BouncyCastle

        /// <summary>
        /// Symmetrical encryption of the text.
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="plainText">Text which should be encrypted</param>
        /// <returns></returns>
        public static string EncryptString(string key, string plainText)
        {
            var keybytes = GetKeyBytes(key);
            var secret = Encoding.UTF8.GetBytes(plainText);
            var result = EncryptBytes(keybytes, secret);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Symmetrical decryption of the text
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="cipherText">Text which should be decrypted</param>
        /// <returns></returns>
        public static string DecryptString(string key, string cipherText)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key can not be null or empty.");
            var keybytes = GetKeyBytes(key);
            if (!SecurityUtils.IsBase64String(cipherText))
                throw new ArgumentException("cipherText is not valid. It must be Base64 string");
            var secret = Convert.FromBase64String(cipherText);
            var result = DecryptBytes(keybytes, secret);
            return Encoding.UTF8.GetString(result).Trim('\0');
        }

        public static byte[] EncryptBytes(string key, byte[] secret, byte[] iv = null)
        {
            var keybytes = GetKeyBytes(key);
            var result = EncryptBytes(keybytes, secret, iv);
            return result;
        }

        /// <summary>
        /// Symmetrical encryption of the bytes
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="bytes">Bytes array which should be encrypted</param>
        /// <returns></returns>
        public static byte[] EncryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            var iv_base64 = string.Empty;
            byte[] inputBytes = secret;
            //SecureRandom random = new SecureRandom();
            if (iv == null)
                iv = new byte[16];
            //random.NextBytes(iv);
            iv_base64 = Convert.ToBase64String(iv);
            string keyStringBase64 = Convert.ToBase64String(key);

            //Set up
            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine), new Pkcs7Padding());
            KeyParameter keyParam = new KeyParameter(Convert.FromBase64String(keyStringBase64));
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, 16);

            // Encrypt
            cipher.Init(true, keyParamWithIV);
            byte[] outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
            int length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
            cipher.DoFinal(outputBytes, length); //Do the final block
            return outputBytes;
        }

        public static byte[] DecryptBytes(string key, byte[] secret, byte[] iv = null)
        {
            var keybytes = GetKeyBytes(key);
            var result = DecryptBytes(keybytes, secret, iv);
            return result;
        }
        /// <summary>
        /// Symmetrical decryption of the bytes
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="bytes">Bytes array which should be decrypted</param>
        /// <returns></returns>
        public static byte[] DecryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            var Keysize = 256 / 8;
            int iterationCount = 1;
            if (iv == null)
                iv = new byte[16];

            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine), new Pkcs7Padding());
            KeyParameter keyParam = new KeyParameter(key);
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, 16);
        
            byte[] outputBytes = secret;
            cipher.Init(false, keyParamWithIV);
            byte[] comparisonBytes = new byte[cipher.GetOutputSize(outputBytes.Length)];
            int length = cipher.ProcessBytes(outputBytes, comparisonBytes, 0);
            cipher.DoFinal(comparisonBytes, length); //Do the final block
            byte[] output = comparisonBytes.Take(comparisonBytes.Length).ToArray();
            return output;
        }
        #endregion
    }
}
