using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class MusicNFT : CommonNFT
    {
        public MusicNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Music;
            TypeText = "NFT Music";
        }

        public override async Task Fill(INFT NFT) 
        {
            await FillCommon(NFT);
        }

        private void ParseSpecific(IDictionary<string, string> meta)
        {

        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                ParsePrice(lastmetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                if (string.IsNullOrEmpty(Link) && !string.IsNullOrEmpty(ImageLink))
                    Link = ImageLink;
            }
        }

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                ParsePrice(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                if (string.IsNullOrEmpty(Link) && !string.IsNullOrEmpty(ImageLink))
                    Link = ImageLink;
            }
        }

        public async Task LoadLastData(Dictionary<string,string> metadata)
        {
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

                ParsePrice(metadata);

                if (string.IsNullOrEmpty(Link) && !string.IsNullOrEmpty(ImageLink))
                    Link = ImageLink;

            }
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            var metadata = await GetCommonMetadata();
            return metadata;
        }
    }
}
