using System;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest
{
    public class CalculateFeeTest
    {
        [Fact]
        public void TestMethod()
        {
            double firstValue = 2;
            double secondValue = 3;
            double expectedOutput = 5;

            double sum = firstValue + secondValue;

            //Assert  
            Assert.Equal(expectedOutput, sum, 1);
        }

        [Fact]
        public void CalculateFeeTestMethod()
        {
            double fee = NeblioTransactionHelpers.CalcFee(2, 2, "Test", false);
            double expectedFee = 10000;

            //Assert  
            Assert.Equal(expectedFee, fee, 15);
        }
    }
}
