using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class GetAddressFromPrivateKeyTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result if an address is not having enough Neblio.
        /// </summary>
        [Fact]
        public void GetAddressFromPrivateKey_Valid_Test()
        {
            string privateKey = "TondMadekiw2kRcyeQsRtm1oQCucN8yU9rstqy2rtrW6y2JDRe29";
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";

            var result = NeblioTransactionHelpers.GetAddressFromPrivateKey(privateKey).Result;

            Assert.True(result.Item1);
            Assert.Equal(address, result.Item2);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if an address is not having enough Neblio.
        /// </summary>
        [Fact]
        public void GetAddressFromPrivateKey_InValid_Test()
        {
            string privateKey = "TondMadekiw2kRcyeQsRtm1oQCucN8yU9rstqy2rtrW6y2JDRe2222222";
            
            var result = NeblioTransactionHelpers.GetAddressFromPrivateKey(privateKey).Result;

            Assert.False(result.Item1);
            Assert.Empty(result.Item2);
        }
    }
}
