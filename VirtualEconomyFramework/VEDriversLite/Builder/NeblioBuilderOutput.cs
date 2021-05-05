using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace VEDriversLite.Builder
{
    public class NeblioBuilderOutput
    {
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

        public double Amount { get; set; } = 0.0001;
        public string Address { get; set; } = string.Empty;
        public bool WithTokens { get; set; } = false;
        public string TokenId { get; set; } = string.Empty;
        public string TokenIdImageUrl { get; set; } = string.Empty;
        public int TokenAmount { get; set; } = 0;
        public BitcoinAddress NBitcoinAddress { get; set; }

        public async Task AddReceiver(BitcoinAddress address)
        {
            NBitcoinAddress = address;
        }

        public async Task AddTokens(string tokenId, int amount, string imageUrl)
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

        public async Task RemoveTokens()
        {
            WithTokens = false;
            TokenAmount = 1;
            TokenId = string.Empty;
            TokenIdImageUrl = string.Empty;
        }
    }
}
