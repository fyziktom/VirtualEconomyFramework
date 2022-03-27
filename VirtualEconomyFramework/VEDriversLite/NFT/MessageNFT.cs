using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Security;

namespace VEDriversLite.NFT
{
    public class MessageNFT : CommonNFT
    {
        public MessageNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Message;
            TypeText = "NFT Message";
        }

        public override async Task Fill(INFT NFT) 
        {
            await FillCommon(NFT);
            IsReceivedMessage = (NFT as MessageNFT).IsReceivedMessage;
            Partner = (NFT as MessageNFT).Partner;
            Encrypt = (NFT as MessageNFT).Encrypt;
            Decrypted = (NFT as MessageNFT).Decrypted;
            ImageData = (NFT as MessageNFT).ImageData;
        }

        public bool Encrypt { get; set; } = true;
        public bool Decrypted { get; set; } = false;
        public string Partner { get; set; } = string.Empty;
        [JsonIgnore]
        public string SharedKey { get; set; } = string.Empty;
        [JsonIgnore]
        public byte[] ImageData { get; set; }

        public bool IsReceivedMessage { get; set; } = false;

        private bool runningDecryption = false;

        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            GetPartnerAsync().GetAwaiter().GetResult();
        }

        private async Task GetPartnerAsync()
        {
            await GetPartner();
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            await GetPartner();
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
                IsLoaded = true;
            }
        }

        public async Task GetLastData()
        {
            await GetPartner();
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
                IsLoaded = true;
            }
        }

        public async Task GetPartner()
        {
            var rec = await NeblioTransactionHelpers.GetTransactionSender(Utxo, TxDetails);
            if (!string.IsNullOrEmpty(rec))
                Partner = rec;
        }

        public async Task GetReceiver()
        {
            var rec = await NeblioTransactionHelpers.GetTransactionReceiver(Utxo, TxDetails);
            if (!string.IsNullOrEmpty(rec))
                Partner = rec;
        }

        /// <summary>
        /// this function will decrypt the NFT if it is possile
        /// It needs the owner Private Key to create shared password which the combination of the sender public key.
        /// </summary>
        /// <param name="secret">Owner Private Key</param>
        /// <param name="decryptEvenOnSameAddress">Set true when you have preset the partner address manually - for example encryption on same address. </param>
        /// <returns>true if success</returns>
        public async Task<bool> Decrypt(NBitcoin.BitcoinSecret secret, bool decryptEvenOnSameAddress = false)
        {
            if (runningDecryption)
                return false;

            if (Decrypted)
                return false;

            if (string.IsNullOrEmpty(Partner))
                return false;//throw new Exception("Cannot decrypt without loaded Partner address.");

            var add = secret.PubKey.GetAddress(NBitcoin.ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);

            if (Partner == add.ToString() && !decryptEvenOnSameAddress)
            {
                IsReceivedMessage = false;
                Partner = await NeblioTransactionHelpers.GetTransactionReceiver(Utxo);
                if (string.IsNullOrEmpty(Partner))
                    return false;//throw new Exception("Cannot decrypt without loaded Partner address.");
            }
            else
            {
                IsReceivedMessage = true;
            }
            
            runningDecryption = true;
            if (await LoadSharedKeyForEncrypt(secret))
            {
                Description = await DecryptProperty(Description, secret, "", Partner, sharedkey: SharedKey);
                Name = await DecryptProperty(Name, secret, "", Partner, sharedkey: SharedKey);
                Text = await DecryptProperty(Text, secret, "", Partner, sharedkey: SharedKey);
                ImageLink = await DecryptProperty(ImageLink, secret, "", Partner, sharedkey: SharedKey);
                Link = await DecryptProperty(Link, secret, "", Partner, sharedkey: SharedKey);
            }
            else
            {
                Console.WriteLine("Cannot decrypt NFT. Shared key not found.");
            }
            Decrypted = true;
            runningDecryption = false;

            return true;
        }

        private async Task<bool> LoadSharedKeyForEncrypt(NBitcoin.BitcoinSecret secret)
        {
            if (string.IsNullOrEmpty(SharedKey))
            {
                var sharedkey = await ECDSAProvider.GetSharedSecret(Partner, secret);
                if (sharedkey.Item1)
                {
                    SharedKey = sharedkey.Item2;
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }
        
        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            var metadata = await GetCommonMetadata();
            if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(receiver))
            {
                //throw new Exception("Wrong input. Must fill all parameters if you want to use metadata encryption.");
            }
            else
            {
                var edescription = string.Empty;
                var ename = string.Empty;
                var etext = string.Empty;
                var eimagelink = string.Empty;
                var elink = string.Empty;
                if (Encrypt)
                {
                    var res = await ECDSAProvider.EncryptStringWithSharedSecret(Description, receiver, key);
                    if (res.Item1)
                        edescription = res.Item2;

                    res = await ECDSAProvider.EncryptStringWithSharedSecret(Name, receiver, key);
                    if (res.Item1)
                        ename = res.Item2;

                    res = await ECDSAProvider.EncryptStringWithSharedSecret(Text, receiver, key);
                    if (res.Item1)
                        etext = res.Item2;

                    res = await ECDSAProvider.EncryptStringWithSharedSecret(ImageLink, receiver, key);
                    if (res.Item1)
                        eimagelink = res.Item2;

                    res = await ECDSAProvider.EncryptStringWithSharedSecret(Link, receiver, key);
                    if (res.Item1)
                        elink = res.Item2;

                }
                else
                {
                    edescription = Description;
                    ename = Name;
                    etext = Text;
                    eimagelink = ImageLink;
                    elink = Link;
                }

                metadata["Name"] = ename;

                if (!string.IsNullOrEmpty(Author))
                    metadata["Author"] = address;
                else
                    metadata["Author"] = Author;

                metadata["Description"] = edescription;
                if (!string.IsNullOrEmpty(etext))
                    metadata["Text"] = etext;
                if (!string.IsNullOrEmpty(eimagelink))
                    metadata["Image"] = eimagelink;
                if (!string.IsNullOrEmpty(elink))
                    metadata["Link"] = elink;
            }

            return metadata;
        }
    }
}
