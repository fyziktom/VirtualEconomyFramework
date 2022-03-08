using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class PrivateKeyValidTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey is empty.
        /// </summary>
        [Fact]
        public void Privatekey_Empty_Test()
        {
            var key = NeblioTransactionHelpers.IsPrivateKeyValid("");  
            Assert.Null(key);            
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey length is less than 52 characters.
        /// </summary>
        [Fact]
        public void Privatekey_LessThan52_Test()
        {
            var key1 = NeblioTransactionHelpers.IsPrivateKeyValid("Test");
            Assert.Null(key1);            
        }

        /// <summary>
        /// Unit test method to verify if system is returning the bitcoin secret object correctly for a valid privateKey.
        /// </summary>
        [Fact]
        public void Privatekey_Valid_Test()
        {           
            string privateKey = "TondMadekiw2kRcyeQsRtm1oQCucN8yU9rstqy2rtrW6y2JDRe29";
            var key2 = NeblioTransactionHelpers.IsPrivateKeyValid(privateKey);
            //Assert  
            Assert.NotNull(key2);

            Assert.Equal(key2.ToWif(), privateKey);
        }
    }
}
