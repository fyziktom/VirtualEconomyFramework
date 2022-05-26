using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using Xunit;
using VEDriversLite.Common;

namespace VEFrameworkUnitTest.DogeCoin
{
    public class HexStringToBytesTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result when HexStringToBytes is null.
        /// </summary>
        [Fact]
        public void HexStringToBytes_Null_Test()
        {
            var paramName = "hexString";

            //Assert
            var exception = Assert.Throws<ArgumentNullException>(() => StringExt.HexStringToBytes(null));
            Assert.Equal(paramName, exception.ParamName);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result when hexString does not have an even length.
        /// </summary>
        [Fact]
        public void HexStringToBytes_Length_Test()
        {
            string message = "hexString must have an even length (Parameter 'hexString')";

            //Assert
            var exception = Assert.Throws<ArgumentException>(() => StringExt.HexStringToBytes("123"));
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Unit test method to verify if system is identifying a valid HexStringToBytes.
        /// </summary>
        //[Fact]
        //public void HexStringToBytes_Valid_Test()
        //{
        //    string hexString = "hexString";

        //    var bytes = DogeTransactionHelpers.HexStringToBytes(hexString);

        //    //Assert
        //    Assert.NotEmpty(bytes);
        //}
    }
}
