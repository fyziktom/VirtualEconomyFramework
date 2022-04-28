using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Security;
using VEFrameworkUnitTest.Cryptography.Common;
using Xunit;

namespace VEFrameworkUnitTest.Cryptography
{
    /// <summary>
    /// Test of the Symetric encryption and decryption
    /// </summary>
    public class SymetricEncryptionTests
    {
        /// <summary>
        /// Encrypt with AES256
        /// </summary>
        [Fact]
        public void EncryptMessageCorrectTest()
        {
            var encryptedMessage = SymetricProvider.EncryptString(FakeDataGenerator.DefaultDto.BasicPassword,
                                                                  FakeDataGenerator.DefaultDto.BasicMessage);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicEncryptedMessage, encryptedMessage);
        }

        /// <summary>
        /// Decrypt with AES256
        /// </summary>
        [Fact]
        public void DecryptMessageCorrectTest()
        {
            var decryptedMessage = SymetricProvider.DecryptString(FakeDataGenerator.DefaultDto.BasicPassword,
                                                                  FakeDataGenerator.DefaultDto.BasicEncryptedMessage);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, decryptedMessage);
        }


        /// <summary>
        /// Empty key fail test
        /// </summary>
        [Fact]
        public void EmptyKeyFailTest()
        {
            string message = "key can not be null or empty.";
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                var decryptedMessage = SymetricProvider.DecryptString("", FakeDataGenerator.DefaultDto.BasicEncryptedMessage);
            });
            Assert.Contains(message, exception.Message);
        }

        /// <summary>
        /// Wrong Shape of Cipher Text Fail test
        /// Must be Base64 string
        /// </summary>
        [Fact]
        public void WrongShapeCipherTextFailTest()
        {
            string message = "cipherText is not valid. It must be Base64 string";
            var exception = Assert.Throws<ArgumentException>(() =>
            {
                var decryptedMessage = SymetricProvider.DecryptString(FakeDataGenerator.DefaultDto.BasicPassword, 
                                                                      FakeDataGenerator.DefaultDto.BasicEncryptedMessageWrongShape);
            });
            Assert.Contains(message, exception.Message);
        }

        /// <summary>
        /// Post change of Cipher Text test
        /// </summary>
        [Fact]
        public void WrongCipherTextFailTest()
        {

            string message = "pad block corrupted";
            var exception = Assert.Throws<Org.BouncyCastle.Crypto.InvalidCipherTextException>(() =>
            {
                var decryptedMessage = SymetricProvider.DecryptString(FakeDataGenerator.DefaultDto.BasicPassword,
                                                                      FakeDataGenerator.DefaultDto.BasicEncryptedMessageChanged);
            });
            Assert.Contains(message, exception.Message);
        }
    }
}
