using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Neblio;
using Xunit;

namespace VEFrameworkUnitTest
{
    public class CheckSpendableNeblioTest : NeblioAccountBase
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result if an address is not having enough Neblio.
        /// </summary>
        [Fact]
        public void CheckSpendableNeblio_InCorrect_Test()
        {
            var count = 2;
            this.Address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWd";
            var result = this.CheckSpendableNeblio(0.23).Result;
            string error = "You dont have Neblio on the address. Probably waiting for more than " + count +
                           " confirmations.";

            Assert.Equal(error, result.Item1);
            Assert.Null(result.Item2);
        }

        /// <summary>
        /// Unit test method to verify if system is returning Utxos if an address has spendable Neblios.
        /// </summary>
        [Fact]
        public void CheckSpendableNeblio_Valid_Test()
        {            
            this.Address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";
            var result1 = this.CheckSpendableNeblio(0.23).Result;
            string ok = "OK";
            Assert.Equal(ok, result1.Item1);
            Assert.NotEmpty(result1.Item2);
        }
    }
}
