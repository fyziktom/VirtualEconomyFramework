using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class ProfileNFT : CommonNFT
    {
        public ProfileNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Profile;
            TypeText = "NFT Profile";
        }

        public int Age { get; set; } = 0;
        public string Surname { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string RelationshipStatus { get; set; } = string.Empty;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            var pnft = NFT as ProfileNFT;
            Surname = pnft.Surname;
            Nickname = pnft.Nickname;
            RelationshipStatus = pnft.RelationshipStatus;
            Age = pnft.Age;
        }


        private void ParseSpecific(IDictionary<string, string> meta)
        {
            if (meta.TryGetValue("Surname", out var surname))
                Surname = surname;
            if (meta.TryGetValue("Nickname", out var nickname))
                Nickname = nickname;
            if (meta.TryGetValue("RelationshipStatus", out var relationshipStatus))
                RelationshipStatus = relationshipStatus;
            if (meta.TryGetValue("Age", out var age))
                Age = Convert.ToInt32(age);
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

                ParseSpecific(nftData.NFTMetadata);
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
                ParseSpecific(nftData.NFTMetadata);
            }
        }

        public async Task LoadLastData(Dictionary<string,string> metadata)
        {
            if (metadata != null)
            {
                ParseCommon(metadata);

                ParsePrice(metadata);

                ParseSpecific(metadata);
            }
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            var metadata = await GetCommonMetadata();
            metadata.Add("Surname", Surname);
            metadata.Add("Nickname", Nickname);
            metadata.Add("Age", Age.ToString());

            return metadata;
        }
    }
}
