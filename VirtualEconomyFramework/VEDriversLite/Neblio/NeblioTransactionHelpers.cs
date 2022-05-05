using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Events;
using VEDriversLite.Dto;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Security;

namespace VEDriversLite
{

    /// <summary>
    /// Main Helper class for the Neblio Blockchain Transactions
    /// </summary>
    public static partial class NeblioTransactionHelpers
    {
        /// <summary>
        /// NBitcoin Instance of Mainet Network of Neblio
        /// </summary>
        public static Network Network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;

        /// <summary>
        /// Conversion ration for Neblio to convert from sat to 1 NEBL
        /// </summary>
        public const double FromSatToMainRatio = 100000000;

        /// <summary>
        /// Minimum number of confirmation to send the transaction
        /// </summary>
        public static int MinimumConfirmations = 2;

        /// <summary>
        /// Tokens Info for all already loaded tokens
        /// </summary>
        public static Dictionary<string, GetTokenMetadataResponse> TokensInfo = new Dictionary<string, GetTokenMetadataResponse>();

        /// <summary>
        /// Create short version of address, 3 chars on start...3 chars on end
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string ShortenAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return string.Empty;
            }

            var shortaddress = address.Substring(0, 3) + "..." + address.Substring(address.Length - 3);
            return shortaddress;
        }
        /// <summary>
        /// Create short version of txid hash, 3 chars on start...3 chars on end
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="withDots">default true. add .... between start and end of the tx hash</param>
        /// <param name="len">Length of the result shortened tx hash</param>
        /// <returns></returns>
        public static string ShortenTxId(string txid, bool withDots = true, int len = 10)
        {
            if (string.IsNullOrEmpty(txid))
            {
                return string.Empty;
            }

            if (txid.Length < 10)
            {
                return txid;
            }

            string txids;
            if (withDots)
            {
                txids = txid.Remove(len / 2, txid.Length - len / 2) + "....." + txid.Remove(0, txid.Length - len / 2);
            }
            else
            {
                txids = txid.Remove(len / 2, txid.Length - len / 2) + txid.Remove(0, txid.Length - len / 2);
            }
            return txids;
        }

        /// <summary>
        /// Check if the private key is valid for the Neblio Network
        /// </summary>
        /// <param name="privatekey"></param>
        /// <returns></returns>
        public static BitcoinSecret IsPrivateKeyValid(string privatekey)
        {
            try
            {
                if (string.IsNullOrEmpty(privatekey) || privatekey.Length < 52 || privatekey[0] != 'T')
                {
                    return null;
                }

                var sec = new BitcoinSecret(privatekey, Network);

                if (sec != null)
                {
                    return sec;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        /// <summary>
        /// Parse the Neblio address from the private key
        /// </summary>
        /// <param name="privatekey"></param>
        /// <returns></returns>
        public static string GetAddressFromPrivateKey(string privatekey)
        {
            try
            {
                var p = IsPrivateKeyValid(privatekey);
                if (p != null)
                {
                    var address = p.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);
                    if (address != null)
                    {
                        return address.ToString();
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Validate if the Neblio address is the correct
        /// </summary>
        /// <param name="neblioAddress"></param>
        /// <returns></returns>
        public static string ValidateNeblioAddress(string neblioAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(neblioAddress) || neblioAddress.Length < 34 || neblioAddress[0] != 'N')
                {
                    return string.Empty;
                }

                BitcoinAddress address = BitcoinAddress.Create(neblioAddress, Network);
                if (!string.IsNullOrEmpty(address.ToString()))
                {
                    return address.ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if the number of the confirmation is enough for doing transactions.
        /// It mainly usefull for UI stuff or console.
        /// </summary>
        /// <param name="confirmations"></param>
        /// <returns></returns>
        public static string IsEnoughConfirmationsForSend(int confirmations)
        {
            if (confirmations > MinimumConfirmations)
            {
                return ">" + MinimumConfirmations.ToString();
            }

            return confirmations.ToString();
        }

        /// <summary>
        /// Returns sended amount of neblio in some transaction. It counts the outputs which was send to input address
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="address">expected address where was nebl send in this tx</param>
        /// <returns></returns>
        public static double GetSendAmount(GetTransactionInfoResponse tx, string address)
        {
            BitcoinAddress addr;
            try
            {
                addr = BitcoinAddress.Create(address, Network);
            }
            catch (Exception)
            {
                const string exceptionMessage = "Cannot get amount of transaction. cannot create receiver address!";
                throw new Exception(exceptionMessage);
            }

            var vinamount = 0.0;
            foreach (var vin in tx.Vin)
            {
                if (vin.Addr == address)
                {
                    vinamount += ((double)vin.Value / FromSatToMainRatio);
                }
            }

            var amount = 0.0;
            foreach (var vout in tx.Vout)
            {
                if (vout.ScriptPubKey.Hex == addr.ScriptPubKey.ToHex())
                {
                    amount += ((double)vout.Value / FromSatToMainRatio);
                }
            }

            amount -= vinamount;

            return Math.Abs(amount);
        }


        /// <summary>
        /// Parse message from the OP_RETURN data in the tx
        /// </summary>
        /// <param name="txinfo"></param>
        /// <returns></returns>
        public static (bool, string) ParseNeblioMessage(GetTransactionInfoResponse txinfo)
        {
            if (txinfo == null)
            {
                return (false, "No input data provided.");
            }

            if (txinfo.Vout == null || txinfo.Vout.Count == 0)
            {
                return (false, "No outputs in transaction.");
            }

            foreach (var o in txinfo.Vout)
            {
                if (!string.IsNullOrEmpty(o.ScriptPubKey.Asm) && o.ScriptPubKey.Asm.Contains("OP_RETURN"))
                {
                    var message = o.ScriptPubKey.Asm.Replace("OP_RETURN ", string.Empty);
                    var bytes = HexStringToBytes(message);
                    var msg = Encoding.UTF8.GetString(bytes);
                    return (true, msg);
                }
            }

            return (false, string.Empty);
        }

        /// <summary>
        /// Convert the hex string to bytes
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
            {
                throw new ArgumentNullException("hexString");
            }

            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("hexString must have an even length", "hexString");
            }

            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }

    }
}
