using System;
using VEDriversLite;
using Xunit;
using Moq;

namespace VEFrameworkUnitTest
{
    public class CalculateFeeTest
    {      
        [Fact]
        public void CalculateFee_Valid_Test()
        {            
            double fee = NeblioTransactionHelpers.CalcFee(2, 2, "Test", false);
            double expectedFee = 10000;

            //Assert  
            Assert.Equal(expectedFee, fee, 15);
        }

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
