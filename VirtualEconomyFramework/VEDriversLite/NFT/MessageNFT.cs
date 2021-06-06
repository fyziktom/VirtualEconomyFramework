using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

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
            IconLink = NFT.IconLink;
            ImageLink = NFT.ImageLink;
            Name = NFT.Name;
            Link = NFT.Link;
            Description = NFT.Description;
            Author = NFT.Author;
            SourceTxId = NFT.SourceTxId;
            NFTOriginTxId = NFT.NFTOriginTxId;
            TypeText = NFT.TypeText;
            Utxo = NFT.Utxo;
            Time = NFT.Time;
            UtxoIndex = NFT.UtxoIndex;
        }

        public bool Encrypt { get; set; } = true;
        public bool Decrypted { get; set; } = false;
        public string Partner { get; set; } = string.Empty;

        public bool IsReceivedMessage { get; set; } = false;

        public override async Task ParseOriginData()
        {
            await GetPartner();
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                if (nftData.NFTMetadata.TryGetValue("Name", out var name))
                    Name = name;
                if (nftData.NFTMetadata.TryGetValue("Description", out var description))
                    Description = description;
                if (nftData.NFTMetadata.TryGetValue("Author", out var author))
                    Author = author;
                if (nftData.NFTMetadata.TryGetValue("Link", out var link))
                    Link = link;
                if (nftData.NFTMetadata.TryGetValue("Tags", out var tags))
                    Tags = tags;
                if (nftData.NFTMetadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (nftData.NFTMetadata.TryGetValue("Type", out var type))
                    TypeText = type;

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
                if (nftData.NFTMetadata.TryGetValue("Name", out var name))
                    Name = name;
                if (nftData.NFTMetadata.TryGetValue("Description", out var description))
                    Description = description;
                if (nftData.NFTMetadata.TryGetValue("Author", out var author))
                    Author = author;
                if (nftData.NFTMetadata.TryGetValue("Link", out var link))
                    Link = link;
                if (nftData.NFTMetadata.TryGetValue("Tags", out var tags))
                    Tags = tags;
                if (nftData.NFTMetadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (nftData.NFTMetadata.TryGetValue("Type", out var type))
                    TypeText = type;

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
        }

        public async Task LoadLastData(Dictionary<string,string> metadata)
        {
            await GetPartner();
            if (metadata != null)
            {
                if (metadata.TryGetValue("Name", out var name))
                    Name = name;
                if (metadata.TryGetValue("Description", out var description))
                    Description = description;
                if (metadata.TryGetValue("Author", out var author))
                    Author = author;
                if (metadata.TryGetValue("Link", out var link))
                    Link = link;
                if (metadata.TryGetValue("Tags", out var tags))
                    Tags = tags;
                if (metadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (metadata.TryGetValue("Type", out var type))
                    TypeText = type;
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
    }
}
