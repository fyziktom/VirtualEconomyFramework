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
        }

        public bool Encrypt { get; set; } = true;
        public bool Decrypted { get; set; } = false;
        public string Partner { get; set; } = string.Empty;

        public bool IsReceivedMessage { get; set; } = false;

        private void ParseSpecific(IDictionary<string, string> meta)
        {

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
            }
        }

        public async Task LoadLastData(Dictionary<string,string> metadata)
        {
            await GetPartner();
            if (metadata != null)
            {
                ParseCommon(metadata);

                if (metadata.TryGetValue("SourceUtxo", out var su))
                {
                    SourceTxId = Utxo;
                    NFTOriginTxId = su;
                }
                else
                {
                    SourceTxId = Utxo;
                    NFTOriginTxId = Utxo;
                }
            }
        }

        public async Task GetPartner()
        {
            var rec = await NeblioTransactionHelpers.GetTransactionSender(Utxo);
            if (!string.IsNullOrEmpty(rec))
                Partner = rec;
        }

        public async Task GetReceiver()
        {
            var rec = await NeblioTransactionHelpers.GetTransactionReceiver(Utxo);
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
            
            var dmsg = await Security.ECDSAProvider.DecryptStringWithSharedSecret(Description, Partner, secret);
            var dname = await Security.ECDSAProvider.DecryptStringWithSharedSecret(Name, Partner, secret);

            if (dmsg.Item1)
                Description = dmsg.Item2;
            if (dname.Item1)
                Name = dname.Item2;

            Decrypted = true;

            return true;
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(receiver))
                throw new Exception("Wrong input. Must fill all parameters if you want to use metadata encryption.");

            var edescription = string.Empty;
            var ename = string.Empty;
            if (Encrypt)
            {
                var res = await ECDSAProvider.EncryptStringWithSharedSecret(Description, receiver, key);
                if (res.Item1)
                    edescription = res.Item2;

                res = await ECDSAProvider.EncryptStringWithSharedSecret(Name, receiver, key);
                if (res.Item1)
                    ename = res.Item2;
            }
            else
            {
                edescription = Description;
                ename = Name;
            }

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT Message");
            metadata.Add("Name", ename);
            metadata.Add("Author", address);
            metadata.Add("Description", edescription);
            metadata.Add("Image", ImageLink);
            metadata.Add("Link", Link);

            return metadata;
        }
    }
}
