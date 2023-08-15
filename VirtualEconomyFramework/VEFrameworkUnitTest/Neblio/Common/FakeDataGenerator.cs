using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VEDriversLite;
using VEDriversLite.NeblioAPI;

namespace VEFrameworkUnitTest.Neblio.Common
{
    public static class FakeDataGenerator
    {
        public static string RandHash()
        {
            var random = new Random();
            var a = random.Next(0, 9);
            var b = random.Next(0, 9);
            var c = random.Next(0, 9);
            var d = random.Next(0, 9);
            var e = random.Next(0, 9);
            var f = random.Next(0, 9);

            return $"cb{a}cec4a{b}c3c6df{c}bf033e7da61a{d}8eedb9a2{e}ff2407c11b247b{f}5f05baff6fb";
        }

        public static string RandTokenId()
        {
            var random = new Random();
            var a = random.Next(0, 9);
            var b = random.Next(0, 9);
            var c = random.Next(0, 9);
            var d = random.Next(0, 9);
            var e = random.Next(0, 9);
            var f = random.Next(0, 9);

            return $"La{a}8e{b}EeXUMx{c}1uyfqk{d}kgVWAQq9yBs4{e}nuQW{f}b";
        }

        public static BitcoinAddress GetAddress(string address = "")
        {
            BitcoinAddress addr = null;
            if (string.IsNullOrEmpty(address))
            {
                Key privateKey = new Key(); // generate a random private key
                PubKey publicKey = privateKey.PubKey;
                BitcoinSecret privateKeyFromNetwork = privateKey.GetBitcoinSecret(NeblioTransactionHelpers.Network);
                address = publicKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network).ToString();
            }

            addr = BitcoinAddress.Create(address, NeblioTransactionHelpers.Network);
            if (addr == null)
                throw new Exception("This is not Neblio address");
            return addr;
        }
        public static (BitcoinAddress,BitcoinSecret) GetKeyAndAddress()
        {
            BitcoinAddress addr = null;

            Key privateKey = new Key(); // generate a random private key
            PubKey publicKey = privateKey.PubKey;
            var secret = privateKey.GetBitcoinSecret(NeblioTransactionHelpers.Network);
            var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network).ToString();
            addr = BitcoinAddress.Create(address, NeblioTransactionHelpers.Network);

            return (addr, secret);
        }

        public static Utxos GetFakeNeblioUtxo(string address = "", 
                                              bool randomIndex = true, 
                                              int maxIndex = 4, 
                                              int index = 0, 
                                              int blocktime = 0,
                                              int maxBlocktime = 1000,
                                              int blockheight = 0,
                                              int value = 0,
                                              int maxValue = 1000000000)
        {
            var utxo = new Utxos();
            var random = new Random();

            var addr = GetAddress(address);

            if (randomIndex)
                index = random.Next(0, maxIndex);
            utxo.Index = index;

            utxo.ScriptPubKey = addr.ScriptPubKey.ToString();

            if (blocktime == 0)
                blocktime = random.Next(0, maxBlocktime);
            utxo.Blocktime = blocktime;

            if (blockheight == 0)
                blockheight = blocktime + 10000;
            utxo.Blockheight = blockheight;

            utxo.Tokens = new List<Tokens>();
            if (value == 0)
                value = random.Next(0, maxValue);
            utxo.Value = value;

            //fake tx to create some real calc hash
            var transaction = Transaction.Create(NeblioTransactionHelpers.Network);
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = new OutPoint(uint256.Parse(RandHash()), (int)utxo.Index),
                ScriptSig = addr.ScriptPubKey,
            });

            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = new OutPoint(uint256.Parse(RandHash()), (int)utxo.Index),
                ScriptSig = addr.ScriptPubKey,
            });
            transaction.Outputs.Add(new Money(1000000), addr.ScriptPubKey);
            transaction.Outputs.Add(new Money(100000), addr.ScriptPubKey);

            utxo.Txid = transaction.GetHash().ToString();

            return utxo;
        }

        public static Utxos GetFakeNeblioTokenUtxo(string address = "",
                                                   string tokenId = "",
                                                   bool randomIndex = true,
                                                   int maxIndex = 4,
                                                   int index = 0,
                                                   int blocktime = 0,
                                                   int maxBlocktime = 1000,
                                                   int blockheight = 0,
                                                   int amount = 0,
                                                   int maxAmount = 1000000)
        {
            var utxo = new Utxos();
            var random = new Random();

            var addr = GetAddress(address);

            if (string.IsNullOrEmpty(tokenId))
                tokenId = RandTokenId();

            if (randomIndex)
                index = random.Next(0, maxIndex);
            utxo.Index = index;

            utxo.ScriptPubKey = addr.ScriptPubKey.ToString();

            if (blocktime == 0)
                blocktime = random.Next(0, maxBlocktime);
            utxo.Blocktime = blocktime;

            if (blockheight == 0)
                blockheight = blocktime + 10000;
            utxo.Blockheight = blockheight;

            utxo.Tokens = new List<Tokens>();
            if (amount == 0)
                amount = random.Next(0, maxAmount);

            utxo.Value = 10000; //all token utxos has this value

            utxo.Tokens.Add(new Tokens()
            {
                Amount = amount,
                TokenId = tokenId,
            });

            //fake tx to create some real calc hash
            var transaction = Transaction.Create(NeblioTransactionHelpers.Network);
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = new OutPoint(uint256.Parse(RandHash()), (int)utxo.Index),
                ScriptSig = addr.ScriptPubKey,
            });

            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = new OutPoint(uint256.Parse(RandHash()), (int)utxo.Index),
                ScriptSig = addr.ScriptPubKey,
            });
            transaction.Outputs.Add(new Money(1000000), addr.ScriptPubKey);
            transaction.Outputs.Add(new Money(100000), addr.ScriptPubKey);

            utxo.Txid = transaction.GetHash().ToString();

            return utxo;
        }

        public static GetAddressInfoResponse GetAddressWithNeblUtxos(string address = "", 
                                                                     int countOfUtxos = 10, 
                                                                     int eachWithValue = 0)
        {
            var utxos = new List<Utxos>();
            var addr = GetAddress(address);

            var response = new GetAddressInfoResponse()
            {
                Address = addr.ToString()
            };

            for (var i = 0; i < countOfUtxos; i++)
            {
                Utxos u = null;
                if (eachWithValue == 0)
                    u = GetFakeNeblioUtxo(addr.ToString());
                else
                    u = GetFakeNeblioUtxo(addr.ToString(), value: eachWithValue);
                utxos.Add(u);
            }
            var us = utxos.OrderBy(u => u.Blockheight).Reverse().ToList();
            response.Utxos = us;

            return response;
        }

        public static Dictionary<string, string> GetMetadata()
        {
            var result = new Dictionary<string, string>();
            result.Add("BrightFuture", "Bright Future");
            result.Add("PositiveOutcome", "Positive Outcome");
            result.Add("SuccessfulVenture", "Successful Venture");
            result.Add("HappyMoments", "Happy Moments");
            result.Add("InspiringJourney", "Inspiring Journey");
            result.Add("CreativeSpark", "Creative Spark");
            result.Add("PowerfulImpact", "Powerful Impact");
            result.Add("JoyfulExperience", "Joyful Experience");
            result.Add("InnovativeIdeas", "Innovative Ideas");
            result.Add("HarmoniousBalance", "Harmonious Balance");

            return result;
        }

    }
}
