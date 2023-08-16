using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Security;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT.DevicesNFTs
{
    /// <summary>
    /// IoT Messagee NFT - specific message NFT dedicated for IoT applications
    /// </summary>
    public class IoTMessageNFT : CommonNFT
    {
        /// <summary>
        /// Constructor of the empty NFT IoT Message
        /// </summary>
        /// <param name="utxo"></param>
        public IoTMessageNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.IoTMessage;
            TypeText = "NFT IoTMessage";
        }
        /// <summary>
        /// Fill the data of the NFT
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT) 
        {
            await FillCommon(NFT);
            IsReceivedMessage = (NFT as IoTMessageNFT).IsReceivedMessage;
            Partner = (NFT as IoTMessageNFT).Partner;
            Encrypt = (NFT as IoTMessageNFT).Encrypt;
            Decrypted = (NFT as IoTMessageNFT).Decrypted;
            ImageData = (NFT as IoTMessageNFT).ImageData;
        }
        /// <summary>
        /// Encrypt the message before the send
        /// </summary>
        public bool Encrypt { get; set; } = true;
        /// <summary>
        /// Has been message already decrypted?
        /// </summary>
        public bool Decrypted { get; set; } = false;
        /// <summary>
        /// Communication partner
        /// </summary>
        public string Partner { get; set; } = string.Empty;
        /// <summary>
        /// Was this message received or sent
        /// </summary>
        public bool IsReceivedMessage { get; set; } = false;

        private bool runningDecryption = false;
        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            GetPartnerAsync().GetAwaiter().GetResult();
        }

        private async Task GetPartnerAsync()
        {
            await GetPartner();
        }
        /// <summary>
        /// Find and parse origin data
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata, GetTransactionInfoResponse txinfo = null)
        {
            await GetPartner();
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo, false, txinfo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
                IsLoaded = true;
            }
        }
        /// <summary>
        /// Get last data of this NFT
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Load communication partner info
        /// </summary>
        /// <returns></returns>
        public async Task GetPartner()
        {
            var rec = await NeblioAPIHelpers.GetTransactionSender(Utxo, TxDetails);
            if (!string.IsNullOrEmpty(rec))
                Partner = rec;
        }
        /// <summary>
        /// Get receiver of this message
        /// </summary>
        /// <returns></returns>
        public async Task GetReceiver()
        {
            var rec = await NeblioAPIHelpers.GetTransactionReceiver(Utxo, TxDetails);
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
                Partner = await NeblioAPIHelpers.GetTransactionReceiver(Utxo);
                if (string.IsNullOrEmpty(Partner))
                    return false;//throw new Exception("Cannot decrypt without loaded Partner address.");
            }
            else
            {
                IsReceivedMessage = true;
            }

            runningDecryption = true;

            Description = await DecryptProperty(Description, secret, "", Partner);
            Name = await DecryptProperty(Name, secret, "", Partner);
            Text = await DecryptProperty(Text, secret, "", Partner);
            ImageLink = await DecryptProperty(ImageLink, secret, "", Partner);
            Link = await DecryptProperty(Link, secret, "", Partner);

            Decrypted = true;
            runningDecryption = false;

            return true;
        }
        /// <summary>
        /// Get the NFT data for the NFT
        /// </summary>
        /// <param name="address">Address of the sender</param>
        /// <param name="key">Private key of the sender for encryption</param>
        /// <param name="receiver">receiver of the NFT</param>
        /// <returns></returns>
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
