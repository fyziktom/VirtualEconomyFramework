using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using VEDriversLite.Builder;
using VEDriversLite.NFT;

namespace VEDriversLite.Security
{
    public static class ECDSAProvider
    {
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
        public static async Task<(bool, string)> GetSharedSecret(string bobAddress, BitcoinSecret secret)
        {
            var bobPubKey = await NFTHelpers.GetPubKeyFromLastFoundTx(bobAddress);
            if (!bobPubKey.Item1)
                return (false, "Cannot find public key, input address did not have any spent transations for key extraction.");

            var sharedKey = bobPubKey.Item2.GetSharedPubkey(secret.PrivateKey);
            if (sharedKey == null || string.IsNullOrEmpty(sharedKey.Hash.ToString()))
                return (false, "Cannot create shared secret from keys.");
            else
                return (true, SecurityUtils.ComputeSha256Hash(sharedKey.Hash.ToString()));
        }
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
                var mesage = await SymetricProvider.DecryptString(key.Item2, emessage);
                return (true, mesage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot decrypt message. " + ex.Message);
                return (false, "Cannot decrypt message. " + ex.Message);
            }
        }
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
                var mesage = await SymetricProvider.DecryptString(key, emessage);
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
