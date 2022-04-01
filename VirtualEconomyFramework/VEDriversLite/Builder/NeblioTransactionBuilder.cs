using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Altcoins;
using Newtonsoft.Json.Linq;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Builder
{
    /// <summary>
    /// Main class of the transaction builder.
    /// </summary>
    public static class NeblioTransactionBuilder
    {
        /// <summary>
        /// Neblio Network instance
        /// </summary>
        public static Network NeblioNetwork { get; set; } = NBitcoin.Altcoins.Neblio.Instance.Mainnet;
        /// <summary>
        /// Minimum amount in the transaction
        /// </summary>
        public static double MinimumAmount { get; } = 10000;
        /// <summary>
        /// Minimum fee
        /// </summary>
        public static double Fee { get; } = 10000;

        /// <summary>
        /// Create raw transaction
        /// </summary>
        /// <param name="utxos"></param>
        /// <param name="receivers"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Transaction> CreateRawTransaction(List<NewTokenTxUtxo> utxos,
                                                              List<NewTokenTxReceiver> receivers,
                                                              List<NewTokenTxMetaField> metadata)
        {

            var dto = new SendTokenRequest();
            try
            {
                dto = NeblioTransactionHelpers.GetSendTokenObject(1, 20000, "DEFAULT", "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8");

                dto.To = new List<To>();

                foreach(var rec in receivers)
                {
                    var r = rec.Address;
                    //throw new Exception(r + ", " + rec.Amount.ToString());
                    dto.To.Add(new To()
                    {
                        Address = rec.Address,
                        Amount = (double)rec.Amount,
                        TokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8"
                    });
                }

                if (metadata != null)
                    foreach (var d in metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            dto.From = null;
            foreach(var utxo in utxos)
                dto.Sendutxo.Add(utxo.Utxo); // it is already with :index

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await NeblioTransactionHelpers.SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, NeblioNetwork, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            return transaction;
        }
    }
}
