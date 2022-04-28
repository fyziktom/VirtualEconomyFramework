using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;

namespace VEFrameworkUnitTest.Cryptography.Common
{
    /// <summary>
    /// Default test object with default values for testing cryptography 
    /// </summary>
    public class CryptographyTestDto
    {
        /// <summary>
        /// Public constructor which loads the NBitcoin objects
        /// </summary>
        public CryptographyTestDto()
        {
            AliceSecret ??= NeblioTransactionHelpers.IsPrivateKeyValid(AliceKeystr);
            AliceBitcoinAddress ??= AliceSecret.PubKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);

            BobSecret ??= NeblioTransactionHelpers.IsPrivateKeyValid(BobKeystr);
            BobBitcoinAddress ??= BobSecret.PubKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
            BobAddress ??= BobBitcoinAddress.ToString();
        }

        #region CommonDefaults
        
        /// <summary>
        /// Basic message for signatures, encryption, etc.
        /// </summary>
        public string BasicMessage { get; set; } = "I like AI :)";
        /// <summary>
        /// Basic message hash (double - sha256)
        /// uint256 hash = NBitcoin.Crypto.Hashes.DoubleSHA256(Encoding.UTF8.GetBytes(BasicMessage));
        /// </summary>
        public string BasicMessageHash { get; set; } = "7652754110490cd5ede6802e2ccee24781f246365df351f78debb812b3b7ec0b";
        /// <summary>
        /// Basic message wrong hash (double - sha256)
        /// Changed start 76527 to 1111
        /// </summary>
        public string BasicMessageWrongHash { get; set; } = "1111754110490cd5ede6802e2ccee24781f246365df351f78debb812b3b7ec0b";

        /// <summary>
        /// Basic password for encryption tests
        /// </summary>
        public string BasicPassword { get; set; } = "1-password!";

        /// <summary>
        /// Basic encrypted message with use of AES256 Symetric Encryption.
        /// Now without salt, etc.
        /// var se = SymetricProvider.EncryptString(BasicPassword, BasicMessage);
        /// https://github.com/fyziktom/VirtualEconomyFramework/blob/fe2a83455b73f56f997b95d041a338d17390a90a/VirtualEconomyFramework/VEDriversLite/Security/SymetricProvider.cs#L33
        /// </summary>
        public string BasicEncryptedMessage { get; set; } = "qKw35jPyvKFuWZlOg3pYTg==";

        /// <summary>
        /// Basic encrypted message with use of AES256 Symetric Encryption.
        /// Now without salt, etc.
        /// Removed == at the end
        /// </summary>
        public string BasicEncryptedMessageWrongShape { get; set; } = "qKw35jPyvKFuWZlOg3pYTg";

        /// <summary>
        /// Basic encrypted message with use of AES256 Symetric Encryption.
        /// Now without salt, etc.
        /// Changed start qKw to 111
        /// </summary>
        public string BasicEncryptedMessageChanged { get; set; } = "11135jPyvKFuWZlOg3pYTg==";


        #endregion

        #region AliceDefaults

        /// <summary>
        /// Private key for Neblio blockchain as string
        /// </summary>
        public string AliceKeystr { get; set; } = "Ts2NxC27aNbmFvcf53iBoNGqTv6RXwQzjZEvtu6ensBj3jn1MetF";
        /// <summary>
        /// Private key for Neblio blockchain as NBitcoin object
        /// </summary>
        public BitcoinSecret AliceSecret { get; set; } = null;
        /// <summary>
        /// Bitcoin Address parsed from the private key
        /// </summary>
        public BitcoinAddress AliceBitcoinAddress { get; set; } = null;
        /// <summary>
        /// Bitcoin Address parsed from the private key as string
        /// </summary>
        public string AliceAddress { get; set; } = "NTcDZL3FFBehbMPznHAkucaHUbhZG12tAJ";

        /// <summary>
        /// Basic message signature
        /// var sign = await ECDSAProvider.SignMessage(BasicMessage, Secret);
        /// </summary>
        public string AliceBasicMessageSignature { get; set; } = "0@RGGB+ko3fjiWPzKJxz+mcb4Jcvla0kGTAz3uXonR4K4XxyYgHRDrfv8ez43NAgZ5/bhjLrSqqmu82f7T0AAOag==";
        /// <summary>
        /// Basic message wrong signature (changed start RGG to BBB)
        /// </summary>
        public string AliceBasicMessageWrongSignature { get; set; } = "0@BBBB+ko3fjiWPzKJxz+mcb4Jcvla0kGTAz3uXonR4K4XxyYgHRDrfv8ez43NAgZ5/bhjLrSqqmu82f7T0AAOag==";

        /// <summary>
        /// Basic message Encrypted with ECDSA encrypted with Alice's public key
        /// Cannot be used for the tests, because start of the encrypted data is random generated pubkey.
        /// It can be used just for the test which fails on other causes.
        /// https://github.com/fyziktom/VirtualEconomyFramework/blob/fe2a83455b73f56f997b95d041a338d17390a90a/VirtualEconomyFramework/VEDriversLite/Security/ECDSAProvider.cs#L247
        /// </summary>
        public string AliceBasicMessageECDSAEncrypted { get; set; } = "QklFMQOXarbMQ4eq47Wt99vaHbix1/2Y+USF21z8KSYMfn1C9jB5DxTSnGnzfY1qvUQwtWnyLK7I8GBlgMvYvf5u1Qnv7D+r91BLab11uXdy7ODUNw==";
        /// <summary>
        /// Shared key between Alice and Bob. EDCH(Alice's private key, Bob's public key)
        /// </summary>
        public string AliceBobEDCHSharedKey { get; set; } = "b4ea50d389a4c6409745c028d728a54107aaf222fe37f6793c6fe4df360618a7";
        /// <summary>
        /// Changed Shared key between Alice and Bob. EDCH(Alice's private key, Bob's public key)
        /// Changed at start b4ea to 1111
        /// </summary>
        public string AliceBobEDCHSharedKeyChanged { get; set; } = "111150d389a4c6409745c028d728a54107aaf222fe37f6793c6fe4df360618a7";

        #endregion


        #region BobDefaults

        public string BobKeystr { get; set; } = "Tq9N6sBpvQhB7sH2SL5zgyFsVoNNZNSHJYBRmekRo7HZqL2YLxDk";
        /// <summary>
        /// Private key for Neblio blockchain as NBitcoin object
        /// </summary>
        public BitcoinSecret BobSecret { get; set; } = null;
        /// <summary>
        /// Bitcoin Address parsed from the private key
        /// </summary>
        public BitcoinAddress BobBitcoinAddress { get; set; } = null;
        /// <summary>
        /// Bitcoin Address parsed from the private key as string
        /// </summary>
        public string BobAddress { get; set; } = string.Empty;

        /// <summary>
        /// Basic message signature
        /// var sign = await ECDSAProvider.SignMessage(BasicMessage, Secret);
        /// </summary>
        public string BobBasicMessageSignature { get; set; } = "0@f8LJF80/81r5VVELufbZx7qL0SwYw5c4cYqYUuO2bq5Vdup0G4T+9FdcNgJ3ebBNj6S9PvIjPZWUDB0wZ+diCA==";
        /// <summary>
        /// Basic message wrong signature (changed start 81r to BBB)
        /// </summary>
        public string BobBasicMessageWrongSignature { get; set; } = "0@BBBJF80/81r5VVELufbZx7qL0SwYw5c4cYqYUuO2bq5Vdup0G4T+9FdcNgJ3ebBNj6S9PvIjPZWUDB0wZ+diCA==";

        #endregion
    }

    /// <summary>
    /// Fake generator for the tests around cryptography
    /// </summary>
    internal static class FakeDataGenerator
    {
        public static CryptographyTestDto DefaultDto { get; set; } = new CryptographyTestDto();
        /// <summary>
        /// Generates a default dto with prepared static values
        /// </summary>
        /// <returns></returns>
        public static CryptographyTestDto GenerateCryptographyTestDto()
        {
            return new CryptographyTestDto();
        }
    }
}
