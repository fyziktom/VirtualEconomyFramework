using System;
using VEDriversLite;
using Xunit;
using Moq;

namespace VEFrameworkUnitTest.Neblio
{
    public class CalculateFeeTest
    {
        /// <summary>
        /// Unit test method to verify if Fee is getting calculated correctly with correct input params.
        /// </summary>
        [Fact]
        public void CalculateFee_Valid_Test()
        {            
            double fee = NeblioTransactionHelpers.CalcFee(2, 2, "Test", false);
            double expectedFee = 10000;

            //Assert  
            Assert.Equal(expectedFee, fee, 15);
        }

        /// <summary>
        /// Unit test method to verify if system is throwing exception correctly when transaction size is exceeding 4 kb.
        /// </summary>
        [Fact]
        public void CalculateFee_MoreThan4kb_Test()
        {
            //Arrange
            string message = "Cannot send transaction bigger than 4kB on Neblio network!";

            //Assert + Action
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.CalcFee(100, 20, "Test custom message", false));
            Assert.Equal(message, exception.Message);
        }
    }
}
