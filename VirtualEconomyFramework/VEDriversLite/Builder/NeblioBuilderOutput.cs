using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace VEDriversLite.Builder
{
    /// <summary>
    /// Neblio Builder Output - representation of the output in the transaction
    /// </summary>
    public class NeblioBuilderOutput
    {
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="tokenAmount"></param>
        /// <param name="tokenId"></param>
        /// <param name="imageUrl"></param>
        /// <exception cref="Exception"></exception>
        public NeblioBuilderOutput(double amount = 0.0001, int tokenAmount = 0, string tokenId = "", string imageUrl = "")
        {
            if (amount < NeblioTransactionBuilder.MinimumAmount)
                throw new Exception($"Amount cannot be lower thant {NeblioTransactionBuilder.MinimumAmount}");

            Amount = amount;
            if (tokenAmount > 0)
            {
                if (string.IsNullOrEmpty(tokenId))
                    throw new Exception("If you want to send tokens, you must fill the token Id.");

                WithTokens = true;
                TokenAmount = tokenAmount;
                TokenId = tokenId;
                TokenIdImageUrl = imageUrl;
            }
        }

        /// <summary>
        /// Amount of the output.
        /// </summary>
        public double Amount { get; set; } = 0.0001;
        /// <summary>
        /// Receiver address
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Output carry tokens
        /// </summary>
        public bool WithTokens { get; set; } = false;
        /// <summary>
        /// Token Id of the tokens
        /// </summary>
        public string TokenId { get; set; } = string.Empty;
        /// <summary>
        /// Token Image Url
        /// </summary>
        public string TokenIdImageUrl { get; set; } = string.Empty;
        /// <summary>
        /// Amount of the tokens in the output
        /// </summary>
        public int TokenAmount { get; set; } = 0;
        /// <summary>
        /// Address of the receiver in the shape of the BitcoinAddress
        /// </summary>
        public BitcoinAddress NBitcoinAddress { get; set; }

        /// <summary>
        /// Add receiver of this output
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void AddReceiver(BitcoinAddress address)
        {
            NBitcoinAddress = address;
        }
        /// <summary>
        /// Add tokens to the output
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="amount"></param>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void AddTokens(string tokenId, int amount, string imageUrl)
        {
            if (amount > 0)
            {
                if (string.IsNullOrEmpty(tokenId))
                    throw new Exception("If you want to send tokens, you must fill the token Id.");

                WithTokens = true;
                TokenAmount = amount;
                TokenId = tokenId;
                TokenIdImageUrl = imageUrl;
            }
        }

        /// <summary>
        /// Remove tokens from the output
        /// </summary>
        /// <returns></returns>
        public void RemoveTokens()
        {
            WithTokens = false;
            TokenAmount = 1;
            TokenId = string.Empty;
            TokenIdImageUrl = string.Empty;
        }
    }
}
