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
    /// Test of the Symetric encryption and decryption
    /// </summary>
    public class SymetricEncryptionTests
    {
        /// <summary>
        /// Encrypt with AES256
        /// </summary>
        [Fact]
        public async Task EncryptMessageCorrectTest()
        {
            var encryptedMessage = await SymetricProvider.EncryptStringAsync(FakeDataGenerator.DefaultDto.BasicPassword,
                                                                  FakeDataGenerator.DefaultDto.BasicMessage, FakeDataGenerator.DefaultDto.IV);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicEncryptedMessage, encryptedMessage);
        }

        /// <summary>
        /// Decrypt with AES256
        /// </summary>
        [Fact]
        public async Task DecryptMessageCorrectTest()
        {
            var decryptedMessage = await SymetricProvider.DecryptStringAsync(FakeDataGenerator.DefaultDto.BasicPassword,
                                                                  FakeDataGenerator.DefaultDto.BasicEncryptedMessage);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, decryptedMessage);
        }


        /// <summary>
        /// Empty key fail test
        /// </summary>
        [Fact]
        public async Task EmptyKeyFailTest()
        {
            string message = "key can not be null or empty.";
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                var decryptedMessage = await SymetricProvider.DecryptStringAsync("", FakeDataGenerator.DefaultDto.BasicEncryptedMessage);
            });
            Assert.Contains(message, exception.Message);
        }

        /// <summary>
        /// Wrong Shape of Cipher Text Fail test
        /// Must be Base64 string
        /// </summary>
        [Fact]
        public async Task WrongShapeCipherTextFailTest()
        {
            string message = "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.";
            var exception = await Assert.ThrowsAsync<FormatException>(async () =>
            {
                    var decryptedMessage = await SymetricProvider.DecryptStringAsync(FakeDataGenerator.DefaultDto.BasicPassword,
                                                                          FakeDataGenerator.DefaultDto.BasicEncryptedMessageWrongShape);           
            });
            Assert.Contains(message, exception.Message);
        }

        /// <summary>
        /// Post change of Cipher Text test
        /// </summary>
        [Fact]
        public async Task WrongCipherTextFailTest()
        {

            string message = "pad block corrupted";
            var exception = await Assert.ThrowsAsync<Org.BouncyCastle.Crypto.InvalidCipherTextException>(async () =>
            {
                var decryptedMessage = await SymetricProvider.DecryptStringAsync(FakeDataGenerator.DefaultDto.BasicPassword,
                                                                      FakeDataGenerator.DefaultDto.BasicEncryptedMessageChanged);
            });
            Assert.Contains(message, exception.Message);
        }
    }
}
