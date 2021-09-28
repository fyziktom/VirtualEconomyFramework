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
        public byte[] ImageData { get; set; }

        public bool IsReceivedMessage { get; set; } = false;

        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            GetPartnerAsync();
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

        public async Task<bool> Decrypt(NBitcoin.BitcoinSecret secret)
        {
            if (Decrypted)
                return false;

            if (string.IsNullOrEmpty(Partner))
                throw new Exception("Cannot decrypt without loaded Partner address.");

            var add = secret.PubKey.GetAddress(NeblioTransactionHelpers.Network);

            if (Partner == add.ToString())
            {
                IsReceivedMessage = false;
                Partner = await NeblioTransactionHelpers.GetTransactionReceiver(Utxo);
                if (string.IsNullOrEmpty(Partner))
                    throw new Exception("Cannot decrypt without loaded Partner address.");
            }
            else
            {
                IsReceivedMessage = true;
            }

            Description = await DecryptProperty(Description, secret);
            Name = await DecryptProperty(Name, secret);
            Text = await DecryptProperty(Text, secret);
            ImageLink = await DecryptProperty(ImageLink, secret);
            Link = await DecryptProperty(Link, secret);

            Decrypted = true;

            return true;
        }

        public async Task<bool> DecryptImageData(NBitcoin.BitcoinSecret secret)
        {
            if (!string.IsNullOrEmpty(ImageLink) && ImageLink.Contains("https://gateway.ipfs.io/ipfs/"))
            {
                var hash = ImageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty);
                try
                {
                    var bytes = await NFTHelpers.IPFSDownloadFromInfuraAsync(hash);
                    var dbytesres = await ECDSAProvider.DecryptBytesWithSharedSecret(bytes, Partner, secret);
                    if (dbytesres.Item1)
                    {
                        ImageData = dbytesres.Item2;
                        var bl = ImageData.Length;
                        return true;
                    }
                    else
                        ImageData = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot download the file from ipfs or decrypt it. " + ex.Message);
                }
            }
            return false;
        }

        private async Task<string> DecryptProperty(string prop, NBitcoin.BitcoinSecret secret)
        {
            if (!string.IsNullOrEmpty(prop))
            {
                try
                {
                    var d = await ECDSAProvider.DecryptStringWithSharedSecret(prop, Partner, secret);
                    if (d.Item1)
                        return d.Item2;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Cannot decrypt property in NFT Message. " + ex.Message);
                    return prop;
                }
            }
            return string.Empty;
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
                metadata["Author"] = address;
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
