using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest
{
    public class PrivateKeyValidTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey is empty.
        /// </summary>
        [Fact]
        public void Privatekey_Empty_Test()
        {
            var key = NeblioTransactionHelpers.IsPrivateKeyValid("").Result;
            Assert.False(key.Item1);  
            Assert.Null(key.Item2);            
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey length is less than 52 characters.
        /// </summary>
        [Fact]
        public void Privatekey_LessThan52_Test()
        {
            var key1 = NeblioTransactionHelpers.IsPrivateKeyValid("Test").Result;
            Assert.False(key1.Item1);
            Assert.Null(key1.Item2);            
        }

        /// <summary>
        /// Unit test method to verify if system is returning the bitcoin secret object correctly for a valid privateKey.
        /// </summary>
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
