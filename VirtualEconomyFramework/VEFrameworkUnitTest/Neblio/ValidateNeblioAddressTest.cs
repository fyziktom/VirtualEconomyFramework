using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class ValidateNeblioAddressTest
    {       
        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey is empty.
        /// </summary>
        [Fact]
        public void ValidateNeblioAddress_Empty_Test()
        {
            var key = NeblioTransactionHelpers.ValidateNeblioAddress("").Result;
            Assert.False(key.Item1);
            Assert.Empty(key.Item2);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey length is less than 52 characters.
        /// </summary>
        [Fact]
        public void ValidateNeblioAddress_LessThan34_Test()
        {
            var key1 = NeblioTransactionHelpers.ValidateNeblioAddress("Test").Result;
            Assert.False(key1.Item1);
            Assert.Empty(key1.Item2);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result when PrivateKey length is less than 52 characters.
        /// </summary>
        [Fact]
        public void ValidateNeblioAddress_FirstLetterNotN_Test()
        {
            var key1 = NeblioTransactionHelpers.ValidateNeblioAddress("TestTestTestTestTestTestTestTestTestTest").Result;
            Assert.False(key1.Item1);
            Assert.Empty(key1.Item2);
        }

        /// <summary>
        /// Unit test method to verify if system is returning the bitcoin secret object correctly for a valid privateKey.
        /// </summary>
        [Fact]
        public void ValidateNeblioAddress_Valid_Test()
        {
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";

            var result = NeblioTransactionHelpers.ValidateNeblioAddress(address).Result;

            Assert.True(result.Item1);
            Assert.Equal(address, result.Item2);
        }
    }
}
