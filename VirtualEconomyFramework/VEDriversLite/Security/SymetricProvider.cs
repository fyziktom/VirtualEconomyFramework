using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER
using Microsoft.JSInterop;
#endif

#if !WASM
// BouncyCastle only needed in non-WASM build.
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
#endif

namespace VEDriversLite.Security
{
    /// <summary>
    /// Provides symmetric encryption / decryption (AES) using either
    /// BouncyCastle (desktop/server) or SubtleCrypto (Blazor WASM).
    /// </summary>
    public static class SymetricProvider
    {
        public static readonly int IVSize = 16;
        public static readonly string IVDivider = "::IV_DIVIDER::";

#if WASM
        public static IJSRuntime jsRuntime { get; set; }
#else
        public static object? jsRuntime { get; set; }
#endif

        public static string JoinIVToString(string etext, byte[] iv)
        {
            if (iv == null)
                return etext;

            var bet = Encoding.UTF8.GetBytes(etext);
            var ivet = new byte[iv.Length + bet.Length + IVDivider.Length];
            iv.CopyTo(ivet, 0);
            Encoding.UTF8.GetBytes(IVDivider).CopyTo(ivet, IVSize);
            bet.CopyTo(ivet, iv.Length + IVDivider.Length);
            return Convert.ToBase64String(ivet);
        }
        public static bool ContainsIV(string etext)
        {
            byte[] etextBytes = Convert.FromBase64String(etext);
            string et = Encoding.UTF8.GetString(etextBytes);
            return et.Contains(IVDivider);
        }
        public static (byte[] iv, string etext) ParseIVFromString(string etext)
        {
            byte[] ivet = Convert.FromBase64String(etext);
            byte[] iv = new byte[IVSize];
            Array.Copy(ivet, 0, iv, 0, IVSize);

            string et = Encoding.UTF8.GetString(ivet, IVSize + IVDivider.Length, ivet.Length - IVSize - IVDivider.Length);
            return (iv, et);
        }


        /// <summary>
        /// Generates a random IV of length IVSize (16 bytes).
        /// </summary>
        public static byte[] GetIV()
        {
#if WASM
            // If needed in WASM, we might just return random data from a pseudo-random approach
            // or we could generate it through JS as well. For simplicity, here's a dummy approach:
            var iv = new byte[IVSize];
            // In real scenario, you might want a better source of randomness in the WASM environment.
            // e.g. call window.crypto.getRandomValues(...) from JSRuntime.
            for (int i = 0; i < IVSize; i++)
            {
                iv[i] = (byte)(DateTime.UtcNow.Ticks >> (i % 8));
            }
            return iv;
#else
            // BouncyCastle-based generation
            SecureRandom random = new SecureRandom();
            var iv = new byte[IVSize];
            random.NextBytes(iv);
            return iv;
#endif
        }

        /// <summary>
        /// Creates a 256-bit key (32 bytes) from the given string by resizing.
        /// </summary>
        private static byte[] GetKeyBytes(string key)
        {
            var keybytes = Encoding.UTF8.GetBytes(key);
            var keySize = 256 / 8; // 32 bytes
            Array.Resize(ref keybytes, keySize);
            return keybytes;
        }

#if WASM
        // ==============================================================
        //   BLAZOR WASM - SubtleCrypto (JavaScript) implementation
        // ==============================================================

        /// <summary>
        /// Encrypts string using SubtleCrypto in the browser.
        /// The result is returned as Base64 string.
        /// </summary>
        public static async Task<string> EncryptStringAsync(
            string key,
            string plainText,
            byte[] iv = null)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            if (iv == null) iv = GetIV();

            // Convert input to bytes
            byte[] dataBytes = Encoding.UTF8.GetBytes(plainText);
            // Convert key to 32 bytes
            byte[] keyBytes = GetKeyBytes(key);

            // Convert to hex for JS
            string keyHex = BitConverter.ToString(keyBytes).Replace("-", "").ToLower();
            string dataHex = BitConverter.ToString(dataBytes).Replace("-", "").ToLower();
            string ivHex   = BitConverter.ToString(iv).Replace("-", "").ToLower();

            // JS function returns encrypted bytes in hex form
            string hexCipher = await jsRuntime.InvokeAsync<string>(
                "encryptAes",
                keyHex,
                dataHex,
                ivHex
            );

            // Convert hex back to raw bytes
            byte[] cipherBytes = FromHexString(hexCipher);

            // Return as base64
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// Decrypts Base64 string using SubtleCrypto in the browser.
        /// </summary>
        public static async Task<string> DecryptStringAsync(
            string key,
            string cipherTextBase64,
            byte[] iv = null)
        {
            if (string.IsNullOrEmpty(cipherTextBase64)) return string.Empty;
            if (iv == null) iv = new byte[IVSize];

            // Convert cipher from base64 to raw bytes
            byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
            // Convert key to 32 bytes
            byte[] keyBytes = GetKeyBytes(key);

            string keyHex    = BitConverter.ToString(keyBytes).Replace("-", "").ToLower();
            string cipherHex = BitConverter.ToString(cipherBytes).Replace("-", "").ToLower();
            string ivHex     = BitConverter.ToString(iv).Replace("-", "").ToLower();

            // JS function returns decrypted text as hex (or as plain text, depending on your implementation)
            string plainHexOrText = await jsRuntime.InvokeAsync<string>(
                "decryptAes",
                keyHex,
                cipherHex,
                ivHex
            );

            // In the example from your code, you are returning text directly, 
            // not hex. So you might do this:
            // return plainHexOrText;
            // If you prefer returning hex, then you'd do a decode:
            // byte[] plainBytes = FromHexString(plainHexOrText);
            // return Encoding.UTF8.GetString(plainBytes);

            return plainHexOrText;
        }

        public static async Task<byte[]> EncryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            var r = await EncryptStringAsync(Convert.ToBase64String(key), Convert.ToBase64String(secret), iv);
            return Convert.FromBase64String(r);
        }

        public static async Task<byte[]> DecryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            var r = await DecryptStringAsync(Convert.ToBase64String(key), Convert.ToBase64String(secret), iv);
            return Convert.FromBase64String(r);
        }

        public static async Task<byte[]> EncryptBytes(string key, byte[] secret, byte[] iv = null)
        {
            var r = await EncryptStringAsync(key, Convert.ToBase64String(secret), iv);
            return Convert.FromBase64String(r);
        }

        public static async Task<byte[]> DecryptBytes(string key, byte[] secret, byte[] iv = null)
        {
            var r = await DecryptStringAsync(key, Convert.ToBase64String(secret), iv);
            return Convert.FromBase64String(r);
        }

        /// <summary>
        /// Helper method: Convert a hex string back to byte[].
        /// </summary>
        private static byte[] FromHexString(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Array.Empty<byte>();

            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

#else
        // ==============================================================
        //   STANDARD .NET - BouncyCastle implementation
        // ==============================================================

        /// <summary>
        /// Encrypts a string with AES-CBC (using BouncyCastle).
        /// Returns Base64 encoded cipher.
        /// </summary>
        public static async Task<string> EncryptStringAsync(string key, string plainText, byte[] iv = null)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            var keybytes = GetKeyBytes(key);
            var secret = Encoding.UTF8.GetBytes(plainText);
            var result = await EncryptBytes(keybytes, secret, iv);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts a Base64 string with AES-CBC (BouncyCastle).
        /// Returns the decrypted plaintext.
        /// </summary>
        public static async Task<string> DecryptStringAsync(string key, string cipherText, byte[] iv = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key can not be null or empty.");
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            var keybytes = GetKeyBytes(key);
            var secret = Convert.FromBase64String(cipherText);
            var result = await DecryptBytes(keybytes, secret, iv);
            return Encoding.UTF8.GetString(result).Trim('\0');
        }

        /// <summary>
        /// Encrypts raw bytes (AES-CBC, BouncyCastle).
        /// </summary>
        public static async Task<byte[]> EncryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            if (iv == null)
            {
                iv = new byte[IVSize];
                new SecureRandom().NextBytes(iv);
            }
            // Convert key to Base64 just for internal use in BouncyCastle
            string keyStringBase64 = Convert.ToBase64String(key);

            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine);
            PaddedBufferedBlockCipher cipher =
                new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());

            // Prepare parameters
            KeyParameter keyParam = new KeyParameter(Convert.FromBase64String(keyStringBase64));
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, IVSize);

            // Encrypt
            cipher.Init(true, keyParamWithIV);
            byte[] outputBytes = new byte[cipher.GetOutputSize(secret.Length)];
            int length = cipher.ProcessBytes(secret, outputBytes, 0);
            cipher.DoFinal(outputBytes, length);
            return outputBytes;
        }

        /// <summary>
        /// Decrypts raw bytes (AES-CBC, BouncyCastle).
        /// </summary>
        public static async Task<byte[]> DecryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            if (iv == null)
                iv = new byte[IVSize];

            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine);
            PaddedBufferedBlockCipher cipher =
                new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());

            KeyParameter keyParam = new KeyParameter(key);
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, IVSize);

            cipher.Init(false, keyParamWithIV);
            byte[] comparisonBytes = new byte[cipher.GetOutputSize(secret.Length)];
            int length = cipher.ProcessBytes(secret, comparisonBytes, 0);
            cipher.DoFinal(comparisonBytes, length);
            byte[] output = comparisonBytes.Take(comparisonBytes.Length).ToArray();
            return output;
        }
#endif
    }
}


/*
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace VEDriversLite.Security
{
    public static class SymetricProvider
    {
        public static readonly int IVSize = 16;
        public static readonly string IVDivider = "::IV_DIVIDER::";

        public static byte[] GetIV()
        {
            SecureRandom random = new SecureRandom();
            var iv = new byte[IVSize];
            random.NextBytes(iv);
            return iv;
        }

        public static string JoinIVToString(string etext, byte[] iv)
        {
            if (iv == null)
                return etext;

            var bet = Encoding.UTF8.GetBytes(etext);
            var ivet = new byte[iv.Length + bet.Length + IVDivider.Length];
            iv.CopyTo(ivet, 0);
            Encoding.UTF8.GetBytes(IVDivider).CopyTo(ivet, IVSize);
            bet.CopyTo(ivet, iv.Length + IVDivider.Length);
            return Convert.ToBase64String(ivet);
        }
        public static bool ContainsIV(string etext)
        {
            byte[] etextBytes = Convert.FromBase64String(etext);
            string et = Encoding.UTF8.GetString(etextBytes);
            return et.Contains(IVDivider);
        }
        public static (byte[] iv, string etext) ParseIVFromString(string etext)
        {
            byte[] ivet = Convert.FromBase64String(etext);
            byte[] iv = new byte[IVSize];
            Array.Copy(ivet, 0, iv, 0, IVSize);

            string et = Encoding.UTF8.GetString(ivet, IVSize + IVDivider.Length, ivet.Length - IVSize - IVDivider.Length);
            return (iv, et);
        }

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
        public static string EncryptString(string key, string plainText, byte[] iv = null)
        {
            var keybytes = GetKeyBytes(key);
            var secret = Encoding.UTF8.GetBytes(plainText);
            var result = EncryptBytes(keybytes, secret, iv);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Symmetrical decryption of the text
        /// </summary>
        /// <param name="key">Password for the encryption</param>
        /// <param name="cipherText">Text which should be decrypted</param>
        /// <returns></returns>
        public static string DecryptString(string key, string cipherText, byte[] iv = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("key can not be null or empty.");
            var keybytes = GetKeyBytes(key);
            if (!SecurityUtils.IsBase64String(cipherText))
                throw new ArgumentException("cipherText is not valid. It must be Base64 string");
            var secret = Convert.FromBase64String(cipherText);
            var result = DecryptBytes(keybytes, secret, iv);
            return Encoding.UTF8.GetString(result).Trim('\0');
        }
        /// <summary>
        /// Symmetrical encryption of the bytes.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
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
        /// <param name="secret">data to encrypt</param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] EncryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            var iv_base64 = string.Empty;
            byte[] inputBytes = secret;
            //SecureRandom random = new SecureRandom();
            if (iv == null)
                iv = new byte[IVSize];
            //random.NextBytes(iv);
            iv_base64 = Convert.ToBase64String(iv);
            string keyStringBase64 = Convert.ToBase64String(key);

            //Set up
            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine), new Pkcs7Padding());
            KeyParameter keyParam = new KeyParameter(Convert.FromBase64String(keyStringBase64));
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, IVSize);

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
        /// <param name="secret">Data to decrypt</param>
        /// <param name="iv"></param>
        /// <returns></returns>
        
        public static byte[] DecryptBytes(byte[] key, byte[] secret, byte[] iv = null)
        {
            var Keysize = 256 / 8;
            int iterationCount = 1;
            if (iv == null)
                iv = new byte[IVSize];

            AesEngine engine = new AesEngine();
            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine), new Pkcs7Padding());
            KeyParameter keyParam = new KeyParameter(key);
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv, 0, IVSize);
        
            byte[] outputBytes = secret;
            cipher.Init(false, keyParamWithIV);
            byte[] comparisonBytes = new byte[cipher.GetOutputSize(outputBytes.Length)];
            int length = cipher.ProcessBytes(outputBytes, comparisonBytes, 0);
            cipher.DoFinal(comparisonBytes, length); //Do the final block
            byte[] output = comparisonBytes.Take(comparisonBytes.Length).ToArray();
            return output;
        }
*/
/*
public static byte[] DecryptBytes(byte[] key, byte[] secret, byte[] iv = null)
{
    // Nastavíme velikost bloku na 128 bitů (16 bajtů)
    const int BlockSize = 16;
    if (iv == null || iv.Length != BlockSize)
    {
        iv = new byte[BlockSize];
        // Vygenerujte IV nebo načtěte z externího zdroje
        // Příklad: Array.Fill(iv, (byte)0); nebo nějaké bezpečné naplnění IV
    }

    AesEngine engine = new AesEngine();
    CbcBlockCipher blockCipher = new CbcBlockCipher(engine); // CBC
    PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());

    KeyParameter keyParam = new KeyParameter(key);
    ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, iv);

    cipher.Init(false, keyParamWithIV); // false = decryption

    byte[] comparisonBytes = new byte[cipher.GetOutputSize(secret.Length)];
    int length = cipher.ProcessBytes(secret, 0, secret.Length, comparisonBytes, 0);
    try
    {
        length += cipher.DoFinal(comparisonBytes, length); // Decrypt final block
    }
    catch (Exception ex)
    {
        // Zpracování výjimky, pokud dešifrování selže
        throw new InvalidOperationException("Decryption failed", ex);
    }

    // Vraťte skutečná dešifrovaná data
    return comparisonBytes.Take(length).ToArray();
}
*/
/*
public static byte[] DecryptBytes(byte[] key, byte[] secret, byte[] iv = null)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = key;
        aes.IV = iv ?? new byte[16]; // nebo nějaké výchozí IV

        using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
        {
            using (var ms = new MemoryStream(secret))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        cs.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }
    }
}
*/
/*
#endregion
}
}
*/