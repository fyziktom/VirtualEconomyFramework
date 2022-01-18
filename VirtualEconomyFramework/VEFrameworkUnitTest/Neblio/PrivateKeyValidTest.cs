using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest
{
    public class PrivateKeyValidTest
    {
        [Fact]
        public void Privatekey_Empty_Test()
        {
            var key = NeblioTransactionHelpers.IsPrivateKeyValid("").Result;
            Assert.False(key.Item1);  
            Assert.Null(key.Item2);            
        }

        [Fact]
        public void Privatekey_LessThan52_Test()
        {
            var key1 = NeblioTransactionHelpers.IsPrivateKeyValid("Test").Result;
            Assert.False(key1.Item1);
            Assert.Null(key1.Item2);            
        }

        [Fact]
        public void Privatekey_Valid_Test()
        {           
            string privateKey = "TondMadekiw2kRcyeQsRtm1oQCucN8yU9rstqy2rtrW6y2JDRe29";
            var key2 = NeblioTransactionHelpers.IsPrivateKeyValid(privateKey).Result;
            //Assert  
            Assert.True(key2.Item1);
            Assert.NotNull(key2.Item2);

            NBitcoin.BitcoinSecret secret = key2.Item2;
            Assert.Equal(secret.ToWif(), privateKey);
        }
    }
}
