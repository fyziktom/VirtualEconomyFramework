using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using Xunit;

namespace VEFrameworkUnitTest
{
    public class GetAddressNeblUtxoTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning Utxos for a valid address and required amount is found.
        /// </summary>
        [Fact]
        public void GetAddressNeblUtxo_Valid_Test()
        {
            //Arrange           
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";
            double amount = 3;

            //ACT
            var Utxos = NeblioTransactionHelpers.GetAddressNeblUtxo(address, 0.0001, amount).Result;

            //Assert  
            Assert.NotEmpty(Utxos);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an empty Utxos list for incorrect addres. Same validation applies when required amount is higer than available Neblios.
        /// </summary>
        [Fact]
        public void GetSendAmount_EmptyUtxos_Test()
        {
            //Arrange
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdhqw"; //Incorrect Address
            double amount = 3;

            //ACT
            var Utxos = NeblioTransactionHelpers.GetAddressNeblUtxo(address, 0.0001, amount).Result;

            //Assert  
            Assert.Empty(Utxos);
        }
    }
}
