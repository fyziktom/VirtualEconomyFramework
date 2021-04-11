using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Tokens
{
    public class NeblioNTP1Token : CommonToken
    {
        public NeblioNTP1Token(string tokenId)
        {
            Type = TokenTypes.NTP1;
            Id = tokenId;

            if (!string.IsNullOrEmpty(tokenId))
                GetDetails().GetAwaiter().GetResult();
        }

        public override async Task<string> GetDetails()
        {
            var tok = await NeblioTransactionHelpers.TokenMetadataAsync(Type, Id, string.Empty);

            ActualBalance = tok.ActualBalance;
            MaxSupply = tok.MaxSupply;
            Symbol = tok.Symbol;
            Name = tok.Name;
            IssuerName = tok.IssuerName;
            Metadata = tok.Metadata;
            ImageUrl = tok.ImageUrl;
            MetadataAvailable = tok.MetadataAvailable;

            return await Task.FromResult("OK");
        }
    }
}
