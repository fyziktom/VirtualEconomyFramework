using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using VEDriversLite;

namespace VEFrameworkUnitTest.DogeCoin
{
    public class CalcFeeTest
    {
        /// <summary>
        /// Unit test method to verify if Fee is getting calculated correctly with correct input params.
        /// </summary>
        [Fact]
        public void CalculateFee_Valid_Test()
        {
            double fee = DogeTransactionHelpers.CalcFee(2, 2, "Test", false);
            double expectedFee = 525000;

            //Assert  
            Assert.Equal(expectedFee, fee, 15);
        }

        /// <summary>
        /// Unit test method to verify if system is throwing exception correctly when transaction size is exceeding 4 kb.
        /// </summary>
        [Fact]
        public void CalculateFee_MoreThan10kb_Test()
        {
            //Arrange
            string message = "Cannot send transaction bigger than 10kB on DOGE network!";

            //Assert + Action
            var exception = Assert.Throws<Exception>(() => DogeTransactionHelpers.CalcFee(100, 20, "Test custom message", false));
            Assert.Equal(message, exception.Message);
        }
    }
}
