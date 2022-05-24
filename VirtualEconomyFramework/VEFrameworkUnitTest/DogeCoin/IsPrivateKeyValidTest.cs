using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using VEDriversLite;
using VEDriversLite.DogeAPI;


namespace VEFrameworkUnitTest.DogeCoin
{
    public class IsPrivateKeyValidTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey is empty.
        /// </summary>
        [Fact]
        public void Privatekey_Empty_Test()
        {
            var key = DogeTransactionHelpers.IsPrivateKeyValid("");
            Assert.False(key.Success);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey length is less than 52 characters.
        /// </summary>
        [Fact]
        public void Privatekey_LessThan52_Test()
        {
            var key1 = DogeTransactionHelpers.IsPrivateKeyValid("Test");
            Assert.False(key1.Success);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey length doesnot start with Q.
        /// </summary>
        [Fact]
        public void Privatekey_FirstLetterNotQ_Test()
        {
            var key1 = DogeTransactionHelpers.IsPrivateKeyValid("TestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTestTest");
            Assert.False(key1.Success);
        }

        /// <summary>
        /// Unit test method to verify if system is returning the bitcoin secret object correctly for a valid privateKey.
        /// </summary>
        //[Fact]
        //public void Privatekey_Valid_Test()
        //{
        //    string privateKey = "TondMadekiw2kRcyeQsRtm1oQCucN8yU9rstqy2rtrW6y2JDRe29";
        //    var key2 = DogeTransactionHelpers.IsPrivateKeyValid(privateKey);
        //    //Assert  
        //    Assert.True(key2.Success);
        //    Assert.NotNull(key2.Value);
        //}
    }
}
