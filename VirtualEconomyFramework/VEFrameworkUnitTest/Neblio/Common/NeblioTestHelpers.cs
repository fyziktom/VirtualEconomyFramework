using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using VEDriversLite.NeblioAPI;

namespace VEFrameworkUnitTest.Neblio.Common
{
    public static class NeblioTestHelpers
    {
        public static Mock<IClient> Client = new Mock<IClient>();

        public static void CleanNeblioTransactionHelpersCache()
        {
            NeblioTransactionHelpers.AddressInfoCache = new ConcurrentDictionary<string, (DateTime, GetAddressInfoResponse)>();
            NeblioTransactionHelpers.TransactionInfoCache = new ConcurrentDictionary<string, GetTransactionInfoResponse>();
            NeblioTransactionHelpers.TokenTxMetadataCache = new ConcurrentDictionary<string, GetTokenMetadataResponse>(); 
        }

    }
}
