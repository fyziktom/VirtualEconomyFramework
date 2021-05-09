using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using VEDriversLite.Builder;

namespace VEDriversLite.Security
{
    public static class ECDSAProvider
    {
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

        public static async Task<(bool, string)> EncryptMessage(string message, string privateKey)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(privateKey))
                return (false, "Input parameters cannot be empty or null.");

            try
            {
                PubKey k = new PubKey(privateKey);
                var cmsg = k.Encrypt(message);
                return (true, cmsg);
            }
            catch (Exception ex)
            {
                return (false, "Wrong input. Cannot sign the message.");
            }
        }
    }
}
