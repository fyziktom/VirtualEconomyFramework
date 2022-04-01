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
    /// Test of the container for store of the key
    /// </summary>
    public class EncryptionKeysContainerTests
    {
        /// <summary>
        /// Load encrypted Private key and then get private key with use of pass
        /// </summary>
        [Fact]
        public async Task LoadPrivateKeyCorrectTest()
        {
            var ekey = new EncryptionKey(FakeDataGenerator.DefaultDto.BasicEncryptedMessage, "", true);
            var key = ekey.GetEncryptedKey(FakeDataGenerator.DefaultDto.BasicPassword);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, key);
        }

        /// <summary>
        /// Load encrypted Private key and then get private key with use of pass
        /// </summary>
        [Fact]
        public async Task LoadNewPrivateKeyCorrectTest()
        {
            var ekey = new EncryptionKey("", "", true);
            await ekey.LoadNewKey(FakeDataGenerator.DefaultDto.BasicEncryptedMessage, "", true);
            var key = ekey.GetEncryptedKey(FakeDataGenerator.DefaultDto.BasicPassword);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, key);
        }

        /// <summary>
        /// Load encrypted Private key and then load pass
        /// </summary>
        [Fact]
        public async Task LoadPrivateKeyLoadPassCorrectTest()
        {
            var ekey = new EncryptionKey(FakeDataGenerator.DefaultDto.BasicEncryptedMessage, "", true);
            ekey.LoadPassword(FakeDataGenerator.DefaultDto.BasicPassword);
            var key = ekey.GetEncryptedKey(FakeDataGenerator.DefaultDto.BasicPassword);
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, key);
        }

        /// <summary>
        /// Load Private key together with pass
        /// </summary>
        [Fact]
        public async Task LoadPrivateKeyWithPassCorrectTest()
        {
            var ekey = new EncryptionKey(FakeDataGenerator.DefaultDto.BasicEncryptedMessage, "", true);
            ekey.LoadPassword(FakeDataGenerator.DefaultDto.BasicPassword);
            var key = ekey.GetEncryptedKey();
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, key);
        }

        /// <summary>
        /// Load Private key together with pass
        /// </summary>
        [Fact]
        public async Task GetPrivateKeyAfterLockCorrectTest()
        {
            var ekey = new EncryptionKey(FakeDataGenerator.DefaultDto.BasicEncryptedMessage, "", true);
            ekey.LoadPassword(FakeDataGenerator.DefaultDto.BasicPassword);
            var key = ekey.GetEncryptedKey();
            Assert.Equal(FakeDataGenerator.DefaultDto.BasicMessage, key);
            ekey.Lock();
            var nkey = ekey.GetEncryptedKey();
            Assert.Null(nkey);
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
