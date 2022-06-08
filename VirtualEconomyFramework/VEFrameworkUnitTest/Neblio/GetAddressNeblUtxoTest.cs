using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NeblioAPI;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class GetAddressNeblUtxoTest
    {
        
        /// <summary>
        /// Unit test method to verify if system is returning Utxos for a valid address and required amount is found.
        /// </summary>
        [Fact]
        public async void GetAddressNeblUtxo_Valid_Test()
        {
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";
            double amount = 0.015;
            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);
            var u = addressObject.Utxos.FirstOrDefault();
            //ACT
            var Utxos = await NeblioAPIHelpers.GetAddressNeblUtxo(address, 0.0001, amount,addressObject, (double)u.Blockheight);

            //Assert  
            Assert.Equal(2, Utxos.Count);

        }

        /// <summary>
        /// Unit test method to verify if system is returning an empty Utxos list for incorrect addres. Same validation applies when required amount is higer than available Neblios.
        /// </summary>
        [Fact]
        public async void GetSendAmount_EmptyUtxos_Test()
        {
            //Arrange
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";
            double amount = 3;

            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);
            var u = addressObject.Utxos.FirstOrDefault();

            //ACT
            var Utxos = await NeblioAPIHelpers.GetAddressNeblUtxo(address, 0.0001, amount, addressObject, (double)u.Blockheight);

            //Assert  
            Assert.Equal(0, Utxos.Count);
        }
    }
}
