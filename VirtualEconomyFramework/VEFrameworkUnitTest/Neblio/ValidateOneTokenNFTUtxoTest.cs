using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite;
using Xunit;
using Moq;
using System.Linq;

namespace VEFrameworkUnitTest.Neblio
{
    public class ValidateOneTokenNFTUtxoTest
    {
        /// <summary>
        /// Unit test method to verify if system is returning an error result if an address is not having enough Neblio.
        /// </summary>
        [Fact]
        public async void ValidateOneTokenNFTUtxo_Valid_Test()
        {
            var address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";
            var tokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
            var transactionId = string.Empty;
            int index = 1;
            int amount = 1;

            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);
            var tuNFT = Common.FakeDataGenerator.GetFakeNeblioTokenUtxo(address, 
                                                                     tokenId, 
                                                                     randomIndex:false, 
                                                                     index: index, 
                                                                     amount:amount);
            addressObject.Utxos.Add(tuNFT);
            transactionId = tuNFT.Txid;
            var tu = Common.FakeDataGenerator.GetFakeNeblioTokenUtxo(address,
                                                                     tokenId,
                                                                     randomIndex: false,
                                                                     index: index,
                                                                     amount: 100);
            addressObject.Utxos.Add(tu);
            addressObject.Utxos = addressObject.Utxos.OrderBy(u => u.Blockheight).Reverse().ToList();
            var u = addressObject.Utxos.FirstOrDefault();

            var result = await NeblioAPIHelpers.ValidateOneTokenNFTUtxo(address, tokenId, transactionId, index, addressObject, (double)u.Blockheight);

            Assert.Equal(index, result);
        }
    }
}
