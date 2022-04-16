using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Security;
using VEFrameworkUnitTest.Cryptography.Common;
using Xunit;

namespace VEFrameworkUnitTest.Cryptography
{
    public class SignaturesTest
    {
        /// <summary>
        /// Test of creating the hash of the message for the signatures
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HashTheMessage()
        {
            var calchash = NBitcoin.Crypto.Hashes.DoubleSHA256(
                                                  Encoding.UTF8.GetBytes(FakeDataGenerator.DefaultDto.BasicMessage))
                                                  .ToString();
            Assert.Equal(calchash, FakeDataGenerator.DefaultDto.BasicMessageHash);
        }

        /// <summary>
        /// Test to create signature of the message
        /// </summary>
        [Fact]
        public async Task CreateSignatureTest()
        {
            var sign = await ECDSAProvider.SignMessage(FakeDataGenerator.DefaultDto.BasicMessage, 
                                                       FakeDataGenerator.DefaultDto.AliceSecret);

            Assert.True(sign.Item1);
            Assert.Equal(FakeDataGenerator.DefaultDto.AliceBasicMessageSignature, sign.Item2);

            var signb = await ECDSAProvider.SignMessage(FakeDataGenerator.DefaultDto.BasicMessage,
                                                       FakeDataGenerator.DefaultDto.BobSecret);

            Assert.True(signb.Item1);
            Assert.Equal(FakeDataGenerator.DefaultDto.BobBasicMessageSignature, signb.Item2);
        }

        /// <summary>
        /// Test to check correct signature of the message
        /// </summary>
        [Fact]
        public async Task CorrectSignatureTest()
        {
            var alicever = await ECDSAProvider.VerifyMessage(FakeDataGenerator.DefaultDto.BasicMessage, 
                                                         FakeDataGenerator.DefaultDto.AliceBasicMessageSignature, 
                                                         FakeDataGenerator.DefaultDto.AliceSecret.PubKey);

            Assert.True(alicever.Item1);

            var bobver = await ECDSAProvider.VerifyMessage(FakeDataGenerator.DefaultDto.BasicMessage,
                                                         FakeDataGenerator.DefaultDto.BobBasicMessageSignature,
                                                         FakeDataGenerator.DefaultDto.BobSecret.PubKey);

            Assert.True(bobver.Item1);
        }

        /// <summary>
        /// Test to check wrong signature of the message
        /// </summary>
        [Fact]
        public async Task WrongSignatureTest()
        {
            var sign = await ECDSAProvider.VerifyMessage(FakeDataGenerator.DefaultDto.BasicMessage,
                                                         FakeDataGenerator.DefaultDto.AliceBasicMessageWrongSignature,
                                                         FakeDataGenerator.DefaultDto.AliceSecret.PubKey);

            Assert.False(sign.Item1);
        }
        
        /*
        /// <summary>
        /// Test to check correct signature of the message from NeblioAccount object
        /// </summary>
        [Fact]
        public async Task CorrectSignatureWithNeblioAccountTest()
        {
            var na = new NeblioAccount();
            await na.LoadAccount("", FakeDataGenerator.DefaultDto.AliceKeystr, "", false);

            var sign = await na.SignMessage(FakeDataGenerator.DefaultDto.BasicMessage);

            Assert.True(sign.Item1);
            Assert.Equal(FakeDataGenerator.DefaultDto.AliceBasicMessageSignature, sign.Item2);
        }
        
        /// <summary>
        /// Test to check correct signature of the message from NeblioAccount object
        /// </summary>
        [Fact]
        public async Task CorrectSignatureVerificationWithNeblioAccountTest()
        {
            var na = new NeblioAccount();
            await na.LoadAccount("", FakeDataGenerator.DefaultDto.AliceKeystr, "", false);
            
            var sign = await na.VerifyMessage(FakeDataGenerator.DefaultDto.BasicMessage,
                                                         FakeDataGenerator.DefaultDto.AliceBasicMessageSignature,
                                                         FakeDataGenerator.DefaultDto.AliceAddress,
                                                         FakeDataGenerator.DefaultDto.AliceSecret.PubKey);

            Assert.True(sign.Item1);
        }

        /// <summary>
        /// Test to check wrong signature of the message from NeblioAccount object
        /// </summary>
        [Fact]
        public async Task WrongSignatureVerificationWithNeblioAccountTest()
        {
            var na = new NeblioAccount();
            await na.LoadAccount("", FakeDataGenerator.DefaultDto.AliceKeystr, "", false);

            var sign = await na.VerifyMessage(FakeDataGenerator.DefaultDto.BasicMessage,
                                                         FakeDataGenerator.DefaultDto.AliceBasicMessageWrongSignature,
                                                         FakeDataGenerator.DefaultDto.AliceAddress,
                                                         FakeDataGenerator.DefaultDto.AliceSecret.PubKey);

            Assert.False(sign.Item1);
        }
        */
    }
}
