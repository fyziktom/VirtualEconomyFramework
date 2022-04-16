using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.DataEncoders;
using VEDriversLite.Builder;
using VEDriversLite.NFT;

namespace VEDriversLite.Security
{
    /// <summary>
    /// Provider of the ECDSA encryption, signatures, verifications, etc.
    /// </summary>
    public static class ECDSAProvider
    {
        /// <summary>
        /// Verify the Dogecoin message signed by some dogecoin private key
        /// </summary>
        /// <param name="message">input original message</param>
        /// <param name="signature">signature made by some dogecoin address</param>
        /// <param name="address">Dogecoin address</param>
        /// <returns></returns>
        public static async Task<(bool, string)> VerifyDogeMessage(string message, string signature, string address, bool messageIsAlreadyHash = false)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(address))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var add = BitcoinAddress.Create(address, DogeTransactionHelpers.Network);
                //var vadd = (add as IPubkeyHashUsable);

                var split = signature.Split('@');
                if (split.Length > 1)
                {
                    var sg = Convert.FromBase64String(split[1]);
                    var recoveryId = Convert.ToInt32(split[0]);

                    var sgs = new CompactSignature(recoveryId, sg);
                    Console.WriteLine("Signature loaded.");
                    PubKey recoveredPubKey = null;
                    if (!messageIsAlreadyHash)
                    {
                        uint256 hash = NBitcoin.Crypto.Hashes.DoubleSHA256(Encoding.UTF8.GetBytes(message));
                        recoveredPubKey = PubKey.RecoverCompact(hash, sgs);
                    }
                    else
                        recoveredPubKey = PubKey.RecoverCompact(uint256.Parse(message), sgs);

                    var pk = recoveredPubKey.GetAddress(ScriptPubKeyType.Legacy, DogeTransactionHelpers.Network);
                    if (pk.ToString() == add.ToString())
                        return (true, "Verified.");
                }
            }
            catch
            {
                return (false, "Wrong input. Cannot verify the message signature.");
            }
            return (false, "Not verified.");
        }
        /// <summary>
        /// Verify the Neblio message signed by some Neblio private key
        /// </summary>
        /// <param name="message">input original message</param>
        /// <param name="signature">signature made by some Neblio address</param>
        /// <param name="address">Neblio address</param>
        /// <returns></returns>
        public static async Task<(bool, string)> VerifyMessage(string message, string signature, string address, bool messageIsAlreadyHash = false)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(address))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var add = BitcoinAddress.Create(address, NeblioTransactionHelpers.Network);
                //var vadd = (add as IPubkeyHashUsable);
                var split = signature.Split('@');
                if (split.Length > 1)
                {
                    var sg = Convert.FromBase64String(split[1]);
                    var recoveryId = Convert.ToInt32(split[0]);

                    var sgs = new CompactSignature(recoveryId, sg);
                    Console.WriteLine("Signature loaded.");
                    PubKey recoveredPubKey = null;
                    if (!messageIsAlreadyHash)
                    {
                        uint256 hash = NBitcoin.Crypto.Hashes.DoubleSHA256(Encoding.UTF8.GetBytes(message));
                        recoveredPubKey = PubKey.RecoverCompact(hash, sgs);
                    }
                    else
                        recoveredPubKey = PubKey.RecoverCompact(uint256.Parse(message), sgs);
                    
                    var pk = recoveredPubKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
                    if (pk.ToString() == add.ToString())
                        return (true, "Verified.");
                }                    
            }
            catch
            {
                return (false, "Wrong input. Cannot verify the message signature.");
            }
            return (false, "Not verified.");
        }
        /// <summary>
        /// Verify the Neblio message signed by some Neblio Private Key.
        /// This function uses the Public Key instead of the Address for the verification
        /// </summary>
        /// <param name="message">input original message</param>
        /// <param name="signature">signature made by some Neblio address</param>
        /// <param name="address">Neblio address</param>
        /// <returns></returns>
        public static async Task<(bool, string)> VerifyMessage(string message, string signature, PubKey pubkey, bool messageIsAlreadyHash = false)
        {
            //if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(address))
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || pubkey == null)
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                //var add = BitcoinAddress.Create(address, NeblioTransactionHelpers.Network);
                //var vadd = (add as IPubkeyHashUsable);
                var split = signature.Split('@');
                if (split.Length > 1)
                {
                    var sg = Convert.FromBase64String(split[1]);
                    var recoveryId = Convert.ToInt32(split[0]);

                    var sgs = new CompactSignature(recoveryId, sg);
                    Console.WriteLine("Signature loaded.");
                    PubKey recoveredPubKey = null;
                    if (!messageIsAlreadyHash)
                    {
                        uint256 hash = NBitcoin.Crypto.Hashes.DoubleSHA256(Encoding.UTF8.GetBytes(message));
                        recoveredPubKey = PubKey.RecoverCompact(hash, sgs);
                    }
                    else
                        recoveredPubKey = PubKey.RecoverCompact(uint256.Parse(message), sgs);

                    if (recoveredPubKey.ToHex() == pubkey.ToHex())
                        return (true, "Verified.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in verification. " + ex.Message);
                return (false, "Wrong input. Cannot verify the message signature." + ex.Message);
            }
            return (false, "Not verified.");
        }
        /// <summary>
        /// Sign the Message with Neblio Private Key
        /// </summary>
        /// <param name="message">Message to sign</param>
        /// <param name="privateKey">Neblio Private Key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> SignMessage(string message, string privateKey)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(privateKey))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var secret = new BitcoinSecret(privateKey, NeblioTransactionHelpers.Network);
                uint256 hash = NBitcoin.Crypto.Hashes.DoubleSHA256(Encoding.UTF8.GetBytes(message));
                var smsg = secret.PrivateKey.SignCompact(hash);
                string signature = smsg.RecoveryId.ToString() + "@" + Convert.ToBase64String(smsg.Signature);
                return (true, signature);
            }
            catch
            {
                return (false, "Wrong input. Cannot sign the message.");
            }
        }
        /// <summary>
        /// Sign the Message with Neblio Private Key
        /// </summary>
        /// <param name="message">Message to sign</param>
        /// <param name="secret">Neblio Private Key in form of the BitcoinSecret</param>
        /// <returns></returns>
        public static async Task<(bool, string)> SignMessage(string message, BitcoinSecret secret)
        {
            if (string.IsNullOrEmpty(message) || secret == null)
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                uint256 hash = NBitcoin.Crypto.Hashes.DoubleSHA256(Encoding.UTF8.GetBytes(message));
                var smsg = secret.PrivateKey.SignCompact(hash);
                string signature = smsg.RecoveryId.ToString() + "@" + Convert.ToBase64String(smsg.Signature);
                return (true, signature);
            }
            catch
            {
                return (false, "Wrong input. Cannot sign the message.");
            }
        }
        /// <summary>
        /// Decrypt the message with use of the Neblio Private Key
        /// </summary>
        /// <param name="cryptedMessage">Encrypted Message to decrypt</param>
        /// <param name="privateKey">Neblio Private Key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> DecryptMessage(string cryptedMessage, string privateKey)
        {
            if (string.IsNullOrEmpty(cryptedMessage) || string.IsNullOrEmpty(privateKey))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var secret = new BitcoinSecret(privateKey, NeblioTransactionHelpers.Network);
                var msg = secret.PrivateKey.Decrypt(cryptedMessage);

                return (true, msg);
            }
            catch (Exception ex)
            {
                return (false, "Wrong input. Cannot decrypt the message.");
            }
        }
        /// <summary>
        /// Decrypt the message with use of the Neblio Private Key
        /// </summary>
        /// <param name="cryptedMessage">Encrypted Message to decrypt</param>
        /// <param name="secret">Neblio Private Key in form of BitcoinSecret</param>
        /// <returns></returns>
        public static async Task<(bool, string)> DecryptMessage(string cryptedMessage, BitcoinSecret secret)
        {
            if (string.IsNullOrEmpty(cryptedMessage) || secret == null)
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var msg = secret.PrivateKey.Decrypt(cryptedMessage);
                return (true, msg);
            }
            catch (Exception ex)
            {
                return (false, "Wrong input. Cannot decrypt the message.");
            }
        }
        /// <summary>
        /// Encrypt the message with use of Public Key
        /// </summary>
        /// <param name="message">Message to encrypt</param>
        /// <param name="publicKey">Public Key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> EncryptMessage(string message, string publicKey)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(publicKey))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                PubKey k = new PubKey(publicKey);
                var cmsg = k.Encrypt(message);
                return (true, cmsg);
            }
            catch (Exception ex)
            {
                return (false, "Wrong input. Cannot encrypt the message.");
            }
        }

        /// <summary>
        /// Encrypt the message with use of Public Key
        /// </summary>
        /// <param name="message">Message to encrypt</param>
        /// <param name="publicKey">Public Key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> EncryptMessage(string message, PubKey publicKey)
        {
            if (string.IsNullOrEmpty(message) || publicKey == null)
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var cmsg = publicKey.Encrypt(message);
                return (true, cmsg);
            }
            catch
            {
                return (false, "Wrong input. Cannot encrypt the message.");
            }
        }
        
        /// <summary>
        /// Obtain the Shared secret based on the Neblio Address and Neblio Private Key
        /// </summary>
        /// <param name="bobAddress">Neblio Address of the receiver</param>
        /// <param name="key">Private Key of the sender</param>
        /// <param name="bobPublicKey">Bob public key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> GetSharedSecret(string bobAddress, string key, PubKey bobPublicKey = null)
        {
            if (string.IsNullOrEmpty(key) || (!string.IsNullOrEmpty(bobAddress) && bobPublicKey == null))
                return (false, "Input parameters cannot be empty or null.");
            try
            {
               var secret =  new BitcoinSecret(key, NeblioTransactionBuilder.NeblioNetwork);
                return await GetSharedSecret(bobAddress, secret, bobPublicKey);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Wrong input key for creation of shared secret.");
            }
            return (false, "Wrong input Key.");
        }
        /// <summary>
        /// Obtain the Shared secret based on the Neblio Address and Neblio Private Key
        /// </summary>
        /// <param name="bobAddress">Neblio Address of the receiver</param>
        /// <param name="secret">Private Key of the sender in the form of the BitcoinSecret</param>
        /// <param name="bobPublicKey">Bob public key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> GetSharedSecret(string bobAddress, BitcoinSecret secret, PubKey bobPublicKey = null)
        {
            if (secret == null || (!string.IsNullOrEmpty(bobAddress) && bobPublicKey == null))
                return (false, "Input parameters cannot be empty or null.");
            
            var aliceadd = secret.PubKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
            var bpkey = secret.PubKey;
            if (aliceadd.ToString() != bobAddress.ToString())
            {
                if (bobPublicKey == null)
                {
                    var bobPubKey = await NFTHelpers.GetPubKeyFromLastFoundTx(bobAddress);

                    if (!bobPubKey.Item1)
                        return (false, "Cannot find public key, input address did not have any spent transations for key extraction.");
                    else
                        bpkey = bobPubKey.Item2;
                }
                else
                {
                    bpkey = bobPublicKey;
                }
            }

            var sharedKey = bpkey.GetSharedPubkey(secret.PrivateKey);
            if (sharedKey == null || string.IsNullOrEmpty(sharedKey.Hash.ToString()))
                return (false, "Cannot create shared secret from keys.");
            else
                return (true, SecurityUtils.ComputeSha256Hash(sharedKey.Hash.ToString()));
        }
        /// <summary>
        /// Encrypt the string with Shared secret
        /// </summary>
        /// <param name="message">Message to encrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="key">Neblio Private Key of the Sender</param>
        /// <param name="sharedkey">Shared key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> EncryptStringWithSharedSecret(string message, string bobAddress, string key, string sharedkey = "")
        {
            try
            {
                var secret = new BitcoinSecret(key, NeblioTransactionBuilder.NeblioNetwork);
                return await EncryptStringWithSharedSecret(message, bobAddress, secret, sharedkey);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wrong input key for creation of shared secret.");
            }
            return (false, "Wrong input Key.");
        }
        /// <summary>
        /// Encrypt the string with Shared secret
        /// </summary>
        /// <param name="message">Message to encrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="secret">Alice secret</param>
        /// <param name="sharedkey">shared key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> EncryptStringWithSharedSecret(string message, string bobAddress, BitcoinSecret secret, string sharedkey = "")
        {
            if (string.IsNullOrEmpty(message))
                return (false, "Message cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                return (false, "Partner Address cannot be empty or null.");
            if (secret == null)
                return (false, "Input secret cannot null.");

            (bool, string) key = (false, "");
            if (string.IsNullOrEmpty(sharedkey))
            {
                key = await GetSharedSecret(bobAddress, secret);
                if (!key.Item1)
                    return key;
            }
            else
                key.Item2 = sharedkey;

            try
            {
                var emesage = SymetricProvider.EncryptString(key.Item2, message);
                return (true, emesage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot encrypt message. " + ex.Message);
                return (false, "Cannot encrypt message. " + ex.Message);
            }
        }
        /// <summary>
        /// Encrypt the bytes with Shared secret
        /// </summary>
        /// <param name="inputBytes">Array of the bytes to encrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="secret">Neblio Private Key of the Sender in the form of BitcoinSecret</param>
        /// <param name="sharedkey">shared key</param>
        /// <returns></returns>
        public static async Task<(bool, byte[])> EncryptBytesWithSharedSecret(byte[] inputBytes, string bobAddress, BitcoinSecret secret, string sharedkey = "")
        {
            if (inputBytes == null || inputBytes.Length == 0)
                throw new Exception("Input cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                throw new Exception("Partner Address cannot be empty or null.");
            if (secret == null)
                throw new Exception("Input secret cannot null.");

            (bool, string) key = (false, "");
            if (string.IsNullOrEmpty(sharedkey))
            {
                key = await GetSharedSecret(bobAddress, secret);
                if (!key.Item1)
                    return (false, null);
            }
            else
                key.Item2 = sharedkey;

            try
            {
                var ebytes = SymetricProvider.EncryptBytes(key.Item2, inputBytes);
                return (true, ebytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot encrypt bytes. " + ex.Message);
                throw new Exception("Cannot encrypt bytes. " + ex.Message);
            }
        }

        /// <summary>
        /// Encrypt the string with Shared secret
        /// </summary>
        /// <param name="message">Message to encrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="key">Shared secret key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> EncryptStringWithSharedSecretWithKey(string message, string bobAddress, string key)
        {
            if (string.IsNullOrEmpty(message))
                return (false, "Message cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                return (false, "Partner Address cannot be empty or null.");
            if (string.IsNullOrEmpty(key))
                return (false, "Input secret cannot null.");

            try
            {
                var emesage = SymetricProvider.EncryptString(key, message);
                return (true, emesage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot encrypt message. " + ex.Message);
                return (false, "Cannot encrypt message. " + ex.Message);
            }
        }

        /// <summary>
        /// Decrypt the string with Shared secret
        /// </summary>
        /// <param name="emessage">Message to decrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="key">Shared secret key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> DecryptStringWithSharedSecret(string emessage, string bobAddress, string key)
        {
            try
            {
                var secret = new BitcoinSecret(key, NeblioTransactionBuilder.NeblioNetwork);
                return await DecryptStringWithSharedSecret(emessage, bobAddress, secret);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wrong input key for creation of shared secret.");
            }
            return (false, "Wrong input Key.");
        }
        /// <summary>
        /// Decrypt the string with Shared secret
        /// </summary>
        /// <param name="emessage">Message to decrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="secret">Neblio Private Key in form of the BitcoinSecret</param>
        /// <param name="sharedkey">Shared key from some previous call</param>
        /// <returns></returns>
        public static async Task<(bool, string)> DecryptStringWithSharedSecret(string emessage, string bobAddress, BitcoinSecret secret, string sharedkey = "")
        {
            if (string.IsNullOrEmpty(emessage))
                return (false, "Message cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                return (false, "Partner Address cannot be empty or null.");
            if (secret == null)
                return (false, "Input secret cannot null.");

            (bool, string) key = (false, "");
            if (string.IsNullOrEmpty(sharedkey))
            {
                key = await GetSharedSecret(bobAddress, secret);
                if (!key.Item1)
                    return key;
            }
            else
                key = (true, sharedkey);
            
            try
            {
                var mesage = SymetricProvider.DecryptString(key.Item2, emessage);
                return (true, mesage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot decrypt message. " + ex.Message);
                return (false, "Cannot decrypt message. " + ex.Message);
            }
        }
        
        /// <summary>
        /// Decrypt the bytes with Shared secret
        /// </summary>
        /// <param name="ebytes">Array of the bytes to decrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="secret">Neblio Private Key of the Sender in the form of BitcoinSecret</param>
        /// <param name="sharedkey">Shared key from some previous call</param>
        /// <returns></returns>
        public static async Task<(bool, byte[])> DecryptBytesWithSharedSecret(byte[] ebytes, string bobAddress, BitcoinSecret secret, string sharedkey = "")
        {
            if (ebytes == null || ebytes.Length == 0)
                throw new Exception("Input cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                throw new Exception("Partner Address cannot be empty or null.");
            if (secret == null)
                throw new Exception("Input secret cannot null.");

            (bool, string) key = (false, "");
            if (string.IsNullOrEmpty(sharedkey))
            {
                key = await GetSharedSecret(bobAddress, secret);
                if (!key.Item1)
                    return (false, null);
            }
            else
                key = (true, sharedkey);

            try
            {
                var bytes = SymetricProvider.DecryptBytes(key.Item2, ebytes);
                return (true, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot decrypt bytes. " + ex.Message);
                return (false, null);
            }
        }

        /// <summary>
        /// Dencrypt the string with Shared secret
        /// </summary>
        /// <param name="emessage">Message to decrypt</param>
        /// <param name="bobAddress">Receiver Neblio Address</param>
        /// <param name="key">Shared secret key</param>
        /// <returns></returns>
        public static async Task<(bool, string)> DecryptStringWithSharedSecretWithKey(string emessage, string bobAddress, string key)
        {
            if (string.IsNullOrEmpty(emessage))
                return (false, "Message cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                return (false, "Partner Address cannot be empty or null.");
            if (string.IsNullOrEmpty(key))
                return (false, "Input secret cannot null.");

            try
            {
                var mesage = SymetricProvider.DecryptString(key, emessage);
                return (true, mesage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot decrypt message. " + ex.Message);
                return (false, "Cannot decrypt message. " + ex.Message);
            }
        }



        //////////////////////////////////////////////////////////
        // taken from here https://github.com/MetacoSA/NBitcoin/blob/cb6d64664b9f38d0442a4a4ad5edaed9e310bf40/NBitcoin/Key.cs#L264
        // we need it now even with BouncyCastle. Build of the NBitcoin use msft security for aes which does not work in blazor wasm
        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string DecryptStringWithPrivateKey(string encryptedText, Key privateKey)
        {
            if (encryptedText is null)
                throw new ArgumentNullException(nameof(encryptedText));
            
            var bytes = Encoders.Base64.DecodeData(encryptedText);
            var decrypted = DecryptBytesWithPrivateKey(bytes, privateKey);
            return Encoding.UTF8.GetString(decrypted, 0, decrypted.Length).Trim('\0');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encrypted"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] DecryptBytesWithPrivateKey(byte[] encrypted, Key privateKey)
        {
            if (encrypted is null)
                throw new ArgumentNullException(nameof(encrypted));
            if (encrypted.Length < 85)
                throw new ArgumentException("Encrypted text is invalid, it should be length >= 85.");
            
            var magic = encrypted.SafeSubarray(0, 4);
            var ephemeralPubkeyBytes = encrypted.SafeSubarray(4, 33);
            var cipherText = encrypted.SafeSubarray(37, encrypted.Length - 32 - 37);
            var mac = encrypted.SafeSubarray(encrypted.Length - 32);
            if (!Utils.ArrayEqual(magic, Encoders.ASCII.DecodeData("BIE1")))
                throw new ArgumentException("Encrypted text is invalid, Invalid magic number.");

            var ephemeralPubkey = new PubKey(ephemeralPubkeyBytes);

            var sharedKey = NBitcoin.Crypto.Hashes.SHA512(ephemeralPubkey.GetSharedPubkey(privateKey).ToBytes());
            var iv = sharedKey.SafeSubarray(0, 16);
            var encryptionKey = sharedKey.SafeSubarray(16, 16);
            var hashingKey = sharedKey.SafeSubarray(32);

            var hashMAC = HMACSHA256(hashingKey, encrypted.SafeSubarray(0, encrypted.Length - 32));
            if (!Utils.ArrayEqual(mac, hashMAC))
                throw new ArgumentException("Encrypted text is invalid, Invalid mac.");

            var message = SymetricProvider.DecryptBytes(encryptionKey, cipherText, iv);
            return message;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string EncryptStringWithPublicKey(string message, PubKey key)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            var bytes = Encoding.UTF8.GetBytes(message);
            return Encoders.Base64.EncodeData(EncryptBytesWithPublicKey(bytes, key));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] EncryptBytesWithPublicKey(byte[] message, PubKey key)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            var ephemeral = new Key();
            var sharedKey = NBitcoin.Crypto.Hashes.SHA512(key.GetSharedPubkey(ephemeral).ToBytes());
            var iv = sharedKey.SafeSubarray(0, 16);
            var encryptionKey = sharedKey.SafeSubarray(16, 16);
            var hashingKey = sharedKey.SafeSubarray(32);

            //var aes = new AesBuilder().SetKey(encryptionKey).SetIv(iv).IsUsedForEncryption(true).Build();
            //var cipherText = aes.Process(message, 0, message.Length);
            var cipherText = SymetricProvider.EncryptBytes(encryptionKey, message, iv);
            var ephemeralPubkeyBytes = ephemeral.PubKey.ToBytes();
            var encrypted = Encoders.ASCII.DecodeData("BIE1").Concat(ephemeralPubkeyBytes, cipherText);
            var hashMAC = HMACSHA256(hashingKey, encrypted);
            return encrypted.Concat(hashMAC);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] HMACSHA256(byte[] key, byte[] data)
        {
            var mac = new Org.BouncyCastle.Crypto.Macs.HMac(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
            mac.Init(new Org.BouncyCastle.Crypto.Parameters.KeyParameter(key));
            mac.BlockUpdate(data, 0, data.Length);
            byte[] result = new byte[mac.GetMacSize()];
            mac.DoFinal(result, 0);
            return result;
        }
    }

    // taken from https://github.com/MetacoSA/NBitcoin/blob/cb6d64664b9f38d0442a4a4ad5edaed9e310bf40/NBitcoin/Utils.cs#L473
    internal static class ByteArrayExtensions
    {
        internal static bool StartWith(this byte[] data, byte[] versionBytes)
        {
            if (data.Length < versionBytes.Length)
                return false;
            for (int i = 0; i < versionBytes.Length; i++)
            {
                if (data[i] != versionBytes[i])
                    return false;
            }
            return true;
        }
        internal static byte[] SafeSubarray(this byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0 || offset > array.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0 || offset + count > array.Length)
                throw new ArgumentOutOfRangeException("count");
            if (offset == 0 && array.Length == count)
                return array;
            var data = new byte[count];
            Buffer.BlockCopy(array, offset, data, 0, count);
            return data;
        }

        internal static byte[] SafeSubarray(this byte[] array, int offset)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0 || offset > array.Length)
                throw new ArgumentOutOfRangeException("offset");

            var count = array.Length - offset;
            var data = new byte[count];
            Buffer.BlockCopy(array, offset, data, 0, count);
            return data;
        }

        // https://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
        internal static byte[] Concat(this byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
        internal static byte[] Concat(this byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }


    }
}
