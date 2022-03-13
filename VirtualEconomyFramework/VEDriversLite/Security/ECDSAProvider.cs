using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using VEDriversLite.Builder;
using VEDriversLite.NFT;

namespace VEDriversLite.Security
{
    public static class ECDSAProvider
    {
        /// <summary>
        /// Verify the Dogecoin message signed by some dogecoin private key
        /// </summary>
        /// <param name="message">input original message</param>
        /// <param name="signature">signature made by some dogecoin address</param>
        /// <param name="address">Dogecoin address</param>
        /// <returns></returns>
        public static async Task<(bool, string)> VerifyDogeMessage(string message, string signature, string address)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(address))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var add = BitcoinAddress.Create(address, DogeTransactionHelpers.Network);
                var vadd = (add as IPubkeyHashUsable);

                if (vadd.VerifyMessage(message, signature))
                    return (true, "Verified.");
                else
                    return (false, "Not verified.");
            }
            catch (Exception ex)
            {
                return (false, "Wrong input. Cannot verify the message signature.");
            }
        }
        /// <summary>
        /// Verify the Neblio message signed by some Neblio private key
        /// </summary>
        /// <param name="message">input original message</param>
        /// <param name="signature">signature made by some Neblio address</param>
        /// <param name="address">Neblio address</param>
        /// <returns></returns>
        public static async Task<(bool, string)> VerifyMessage(string message, string signature, string address)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(address))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                var add = BitcoinAddress.Create(address, NeblioTransactionHelpers.Network);
                var vadd = (add as IPubkeyHashUsable);

                if (vadd.VerifyMessage(message, signature))
                    return (true, "Verified.");
                else
                    return (false, "Not verified.");
            }
            catch (Exception ex)
            {
                return (false, "Wrong input. Cannot verify the message signature.");
            }
        }
        /// <summary>
        /// Verify the Neblio message signed by some Neblio Private Key.
        /// This function uses the Public Key instead of the Address for the verification
        /// </summary>
        /// <param name="message">input original message</param>
        /// <param name="signature">signature made by some Neblio address</param>
        /// <param name="address">Neblio address</param>
        /// <returns></returns>
        public static async Task<(bool, string)> VerifyMessage(string message, string signature, PubKey pubkey)
        {
            //if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(address))
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(signature) || pubkey == null)
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                //var add = BitcoinAddress.Create(address, NeblioTransactionHelpers.Network);
                //var vadd = (add as IPubkeyHashUsable);

                if (pubkey.VerifyMessage(message, signature))
                    return (true, "Verified.");
                else
                    return (false, "Not verified.");
            }
            catch (Exception ex)
            {
                return (false, "Wrong input. Cannot verify the message signature.");
            }
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
                var smsg = secret.PrivateKey.SignMessage(message);
                return (true, smsg);
            }
            catch (Exception ex)
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
                var smsg = secret.PrivateKey.SignMessage(message);
                return (true, smsg);
            }
            catch (Exception ex)
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
                return (false, "Wrong input. Cannot sign the message.");
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
                return (false, "Wrong input. Cannot sign the message.");
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
                return (false, "Wrong input. Cannot sign the message.");
            }
        }
        /// <summary>
        /// Obtain the Shared secret based on the Neblio Address and Neblio Private Key
        /// </summary>
        /// <param name="bobAddress">Neblio Address of the receiver</param>
        /// <param name="key">Private Key of the sender</param>
        /// <returns></returns>
        public static async Task<(bool, string)> GetSharedSecret(string bobAddress, string key)
        {
            try
            {
               var secret =  new BitcoinSecret(key, NeblioTransactionBuilder.NeblioNetwork);
                return await GetSharedSecret(bobAddress, secret);
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
        /// <returns></returns>
        public static async Task<(bool, string)> GetSharedSecret(string bobAddress, BitcoinSecret secret)
        {
            var aliceadd = secret.PubKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
            var bpkey = secret.PubKey;
            if (aliceadd.ToString() != bobAddress.ToString())
            {
                var bobPubKey = await NFTHelpers.GetPubKeyFromLastFoundTx(bobAddress);

                if (!bobPubKey.Item1)
                    return (false, "Cannot find public key, input address did not have any spent transations for key extraction.");
                else
                    bpkey = bobPubKey.Item2;
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
        /// <returns></returns>
        public static async Task<(bool, string)> EncryptStringWithSharedSecret(string message, string bobAddress, string key)
        {
            try
            {
                var secret = new BitcoinSecret(key, NeblioTransactionBuilder.NeblioNetwork);
                return await EncryptStringWithSharedSecret(message, bobAddress, secret);
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
        /// <param name="key">Neblio Private Key of the Sender in the form of BitcoinSecret</param>
        /// <returns></returns>
        public static async Task<(bool, string)> EncryptStringWithSharedSecret(string message, string bobAddress, BitcoinSecret secret)
        {
            if (string.IsNullOrEmpty(message))
                return (false, "Message cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                return (false, "Partner Address cannot be empty or null.");
            if (secret == null)
                return (false, "Input secret cannot null.");

            var key = await GetSharedSecret(bobAddress, secret);

            if (!key.Item1)
                return key;

            try
            {
                var emesage = await SymetricProvider.EncryptString(key.Item2, message);
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
        /// <returns></returns>
        public static async Task<(bool, byte[])> EncryptBytesWithSharedSecret(byte[] inputBytes, string bobAddress, BitcoinSecret secret)
        {
            if (inputBytes == null || inputBytes.Length == 0)
                throw new Exception("Input cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                throw new Exception("Partner Address cannot be empty or null.");
            if (secret == null)
                throw new Exception("Input secret cannot null.");

            var key = await GetSharedSecret(bobAddress, secret);
            if (!key.Item1)
                return (false,null);

            try
            {
                var ebytes = await SymetricProvider.EncryptBytes(key.Item2, inputBytes);
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
                var emesage = await SymetricProvider.EncryptString(key, message);
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
        /// <returns></returns>
        public static async Task<(bool, string)> DecryptStringWithSharedSecret(string emessage, string bobAddress, BitcoinSecret secret)
        {
            if (string.IsNullOrEmpty(emessage))
                return (false, "Message cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                return (false, "Partner Address cannot be empty or null.");
            if (secret == null)
                return (false, "Input secret cannot null.");

            var key = await GetSharedSecret(bobAddress, secret);
            if (!key.Item1)
                return key;

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
        /// <returns></returns>
        public static async Task<(bool, byte[])> DecryptBytesWithSharedSecret(byte[] ebytes, string bobAddress, BitcoinSecret secret)
        {
            if (ebytes == null || ebytes.Length == 0)
                throw new Exception("Message cannot be empty or null.");
            if (string.IsNullOrEmpty(bobAddress))
                throw new Exception("Partner Address cannot be empty or null.");
            if (secret == null)
                throw new Exception("Input secret cannot null.");

            var key = await GetSharedSecret(bobAddress, secret);
            if (!key.Item1)
                return (false, null);

            try
            {
                var bytes = await SymetricProvider.DecryptBytes(key.Item2, ebytes);
                return (true, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot decrypt message. " + ex.Message);
                throw new Exception("Cannot decrypt message. " + ex.Message);
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
    }
}
