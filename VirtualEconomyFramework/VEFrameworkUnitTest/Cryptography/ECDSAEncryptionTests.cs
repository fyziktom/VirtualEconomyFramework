using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Security;
using VEFrameworkUnitTest.Cryptography.Common;
using Xunit;

namespace VEFrameworkUnitTest.Cryptography
{
    /// <summary>
    /// Test of the ECDSA encryption and decryption
    /// </summary>
    public class ECDSAEncryptionTests
    {
        /// <summary>
        /// Encrypt and decrypt with ECDSA
        /// It must be tested like this, because this kind of the encryption will create new key 
        /// for calculate the shared key. this Key is at the start of the encrypted data.
        /// For more info please explore the NBitcoin function in Key and PubKey classes
        /// </summary>
        [Fact]
        public async Task ECDSA_EncryptDecryptMessageCorrectTest()
        {
            var encryptedMessage = await ECDSAProvider.EncryptMessage(FakeDataGenerator.DefaultDto.BasicMessage,
                                                                FakeDataGenerator.DefaultDto.AliceSecret.PubKey);
            
            Assert.True(encryptedMessage.Item1);

            var decryptedMessage = await ECDSAProvider.DecryptMessage(encryptedMessage.Item2,
                                                                FakeDataGenerator.DefaultDto.AliceSecret);

            Assert.True(decryptedMessage.Item1);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, decryptedMessage.Item2);
        }

        /// <summary>
        /// Empty key test
        /// </summary>
        [Fact]
        public async Task ECDSA_EmptyKeyFailTest()
        {
            string message = "Input parameters cannot be empty or null.";
            var decryptedMessage = await ECDSAProvider.DecryptMessage(FakeDataGenerator.DefaultDto.AliceBasicMessageECDSAEncrypted,
                                                          "");
            Assert.False(decryptedMessage.Item1);
            Assert.Equal(message, decryptedMessage.Item2);
        }

        /// <summary>
        /// Empty cipher test
        /// </summary>
        [Fact]
        public async Task ECDSA_EmptyCipherFailTest()
        {
            string message = "Input parameters cannot be empty or null.";
            var decryptedMessage = await ECDSAProvider.DecryptMessage("", FakeDataGenerator.DefaultDto.AliceSecret);
            Assert.False(decryptedMessage.Item1);
            Assert.Equal(message, decryptedMessage.Item2);
        }

        /// <summary>
        /// Correct shared key calculation test
        /// This is important for the all other SharedKey encryption features. 
        /// It uses the Symetric encryption which is tested in separated test class.
        /// </summary>
        [Fact]
        public async Task ECDSA_SharedSeceredEncryptionCorrectTest()
        {
            var sharedSecret = await ECDSAProvider.GetSharedSecret(FakeDataGenerator.DefaultDto.BobAddress,
                                                                       FakeDataGenerator.DefaultDto.AliceSecret,
                                                                       FakeDataGenerator.DefaultDto.BobSecret.PubKey);
            Assert.True(sharedSecret.Item1);
            Assert.Equal(FakeDataGenerator.DefaultDto.AliceBobEDCHSharedKey, sharedSecret.Item2);
        }

        
        /// <summary>
        /// Empty secret test
        /// </summary>
        [Fact]
        public async Task ECDSA_SharedSeceredEncryptionEmptySecretFailTest()
        {
            var sharedSecret = await ECDSAProvider.GetSharedSecret("", secret:null,
                                                                       FakeDataGenerator.DefaultDto.BobSecret.PubKey);
            Assert.False(sharedSecret.Item1);
            string message = "Input parameters cannot be empty or null.";
            Assert.Equal(message, sharedSecret.Item2);
        }

        /// <summary>
        /// Empty key test
        /// </summary>
        [Fact]
        public async Task ECDSA_SharedSeceredEncryptionEmptyKeyFailTest()
        {
            var sharedSecret = await ECDSAProvider.GetSharedSecret("", key: "",
                                                                       FakeDataGenerator.DefaultDto.BobSecret.PubKey);
            Assert.False(sharedSecret.Item1);
            string message = "Input parameters cannot be empty or null.";
            Assert.Equal(message, sharedSecret.Item2);
        }

    }
}
