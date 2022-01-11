using System;
using Xunit;

namespace VEFrameworkUnitTest
{
    public class CalculateFeeTest
    {
        [Fact]
        public void Test1()
        {
            double firstValue = 2;
            double secondValue = 3;
            double expectedOutput = 6;

            double sum = firstValue + secondValue;

            //Assert  
            Assert.Equal(expectedOutput, sum, 1);
        }
    }
}
