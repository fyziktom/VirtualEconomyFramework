using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite
{
    /// <summary>
    /// Data carrier for sending token
    /// </summary>
    public class IssueTokenTxData
    {
        /// <summary>
        /// Init the metdata dictionary in constructor
        /// </summary>
        public IssueTokenTxData()
        {
            Flags = new IssuanceFlags()
            {
                AggregationPolicy = AggregationPolicy.Aggregatable,
                Divisibility = 7,
                Locked = true
            };
            IssuanceMetadata = new MetadataOfIssuance()
            {
                Data = new Data2()
                {
                    Description = string.Empty,
                    Issuer = string.Empty,
                    TokenName = string.Empty,
                    Urls = new List<tokenUrlCarrier>(),
                    UserData = new UserData4()
                    {
                        Meta = new List<Meta3>()
                    }
                }
            };
        }
        /// <summary>
        /// Address from where token will be send
        /// </summary>
        public string SenderAddress { get; set; } = string.Empty;
        /// <summary>
        /// If the account is locked you can provide password directly in the send token api command
        /// if the account is unlocked or the QT wallet is connected fill empty string
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Address where token will be send
        /// </summary>
        public string ReceiverAddress { get; set; } = string.Empty;
        /// <summary>
        /// Now default values: Aggregable, Divisibility=7, Locked
        /// </summary>
        public IssuanceFlags Flags { get; }
        /// <summary>
        /// Issuance metadata. It contains new token Symbol (up to 5 chars), issuer name, icon link, user metadata, etc.
        /// It must be filled for the issuing the token
        /// </summary>
        public MetadataOfIssuance IssuanceMetadata { get; set; }
        /// <summary>
        /// Amount of the tokens
        /// </summary>
        public ulong Amount { get; set; } = 0;

        /// <summary>
        /// If you wish to add specific neblio utxo as source for the fee
        /// </summary>
        public string NeblUtxo { get; set; } = string.Empty;
        /// <summary>
        /// If this is set and you will provide NeblUtxo, but it is not found in the list of spendable nebl utxos
        /// it will find another spendable utxo
        /// If this is not set and utxo is not found it will throw exception
        /// </summary>
        public bool SendEvenNeblUtxoNotFound { get; set; } = false;
    }
}
