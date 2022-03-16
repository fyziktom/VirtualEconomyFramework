using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using VEDriversLite.NeblioAPI;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class GetSendAmountTest
    {
        /// <summary>
        /// 
        /// </summary>
        private Mock<IClient> _client = new Mock<IClient>();

        public GetSendAmountTest()
        {
            NeblioTransactionHelpers.GetClient(_client.Object);
            NeblioTransactionHelpers.TurnOnCache = false;
        }

        /// <summary>
        /// Unit test method to verify if system is returning amount correctly.
        /// </summary>
        [Fact]
        public async void GetSendAmount_Valid_Test()
        {
            //Arrange
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";
            var addr = NBitcoin.BitcoinAddress.Create(address, NeblioTransactionHelpers.Network);

            var txinfo = new GetTransactionInfoResponse()
            {
                Vin = new List<Vin>() 
                {  
                    new Vin() 
                    {  
                        Value = 10000, 
                        Addr = address 
                    } 
                },
                Vout = new List<Vout>() 
                {  
                    new Vout() 
                    {  
                        Value = 5000, 
                        ScriptPubKey = new ScriptPubKey() 
                        { 
                            Hex = addr.ScriptPubKey.ToHex() 
                        } 
                    } 
                }
            };

            var amount = NeblioTransactionHelpers.GetSendAmount(txinfo, address);

            //Assert  
            Assert.Equal(0.00005, amount);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result for incorrect address and incorrect transactionId.
        /// </summary>
        [Fact]
        public async void GetSendAmount_Exception_Test()
        {
            //Arrange
            var transactionId = "cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6"; //Incorrect transactionId
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThW"; //Incorrect address
            string message = "Cannot get amount of transaction. cannot create receiver address!";

            var transactionObject = new GetTransactionInfoResponse()
            {
                Vin = new List<Vin>(),
                Vout = new List<Vout>()
            };

            //ACT            

            //Assert
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetSendAmount(transactionObject, address));
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result for incorrect address and incorrect transactionId.
        /// </summary>
        [Fact]
        public async void GetSendAmount_EmptyInAndOutVectors_Test()
        {
            //Arrange
            //var transactionId = "cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6"; //Incorrect transactionId
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";

            var emptyTransactionObject = new GetTransactionInfoResponse()
            {
                Vin = new List<Vin>(),
                Vout = new List<Vout>()
            };

            //ACT            
            double amount = NeblioTransactionHelpers.GetSendAmount(emptyTransactionObject, address);

            //Assert
            Assert.Equal(0, amount);
        }
    }
}
