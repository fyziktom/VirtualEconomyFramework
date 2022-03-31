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
    /// <summary>
    /// Test of the creating the keys and the addresses
    /// </summary>
    public class KeysAddressesTests
    {
        /// <summary>
        /// Load Private key from the string to the BitcoinSecret object
        /// </summary>
        [Fact]
        public async Task LoadPrivateKeyCorrectTest()
        {
            var secret = NeblioTransactionHelpers.IsPrivateKeyValid(FakeDataGenerator.DefaultDto.AliceKeystr);
            Assert.NotNull(secret);
            Assert.Equal(FakeDataGenerator.DefaultDto.AliceKeystr, secret.ToString());
        }

        /// <summary>
        /// Get Neblio address from the private key correct test
        /// </summary>
        [Fact]
        public async Task GetNeblioAddressFromKeyCorrectTest()
        {
            var encryptionKey = new EncryptionKey(FakeDataGenerator.DefaultDto.AliceKeystr);
            var address = NeblioTransactionHelpers.GetAddressAndKey(encryptionKey, "");
            Assert.NotNull(address.Item1);
            Assert.NotNull(address.Item2);
            Assert.Equal(FakeDataGenerator.DefaultDto.AliceKeystr, address.Item2.ToString());
            Assert.Equal(FakeDataGenerator.DefaultDto.AliceAddress, address.Item1.ToString());
        }

    }
}
