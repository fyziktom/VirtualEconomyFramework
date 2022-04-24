using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using VEDriversLite.Security;
using Xunit;
using NBitcoin;

namespace VEFrameworkUnitTest.DogeCoin
{
    public class GetAddressFromPrivateKeyTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning True for valid key.
        /// </summary>
        [Fact]
        public void GetAddressFromPrivateKey_Valid_Test()
        {   
            var key = "QNoNA2w58ceiKr8Py6F5FE8ShFqkJr8TxQ5m5YEpUQDXmume8fN9";
            var address = "DD9or9JWdMJBYhPKcYS2BNu4PAUkun6bnW";

            var result = DogeTransactionHelpers.GetAddressFromPrivateKey(key);

            Assert.True(result.Success);
            Assert.NotEmpty(result.Value.ToString());
            Assert.Equal(address, result.Value.ToString());
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if key is not valid.
        /// </summary>
        [Fact]
        public void GetAddressFromPrivateKey_InValid_Test()
        {
            var key = "123";
           
            var result = DogeTransactionHelpers.GetAddressFromPrivateKey(key);

            Assert.False(result.Success);            
        }
    }
}
