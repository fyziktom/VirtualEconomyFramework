using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest
{
    public class GetSendAmountTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning amount correctly.
        /// </summary>
        [Fact]
        public void GetSendAmount_Valid_Test()
        {
            //Arrange
            var transactionId = "cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb";
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";

            //ACT
            var txinfo = NeblioTransactionHelpers.GetTransactionInfo(transactionId).Result;
            var amount = NeblioTransactionHelpers.GetSendAmount(txinfo, address).Result;            

            //Assert  
            Assert.True(amount > 0);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result for incorrect address and incorrect transactionId.
        /// </summary>
        [Fact]
        public void GetSendAmount_Exception_Test()
        {
            //Arrange
            var transactionId = "cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6"; //Incorrect transactionId
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThW"; //Incorrect address
            string message = "Cannot get amount of transaction. cannot create receiver address!";

            //ACT
            var txinfo = NeblioTransactionHelpers.GetTransactionInfo(transactionId).Result;

            //Assert
            var exception = Assert.ThrowsAsync<Exception>(() => NeblioTransactionHelpers.GetSendAmount(txinfo, address)).Result;
            Assert.Equal(message, exception.Message);
        }
    }
}
