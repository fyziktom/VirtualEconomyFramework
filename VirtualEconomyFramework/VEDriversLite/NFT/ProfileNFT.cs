using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Profile NFT
    /// </summary>
    public class ProfileNFT : CommonNFT
    {
        /// <summary>
        /// Create empty NFT class
        /// </summary>        
        public ProfileNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Profile;
            TypeText = "NFT Profile";
        }
        /// <summary>
        /// Age of the entity
        /// </summary>
        public int Age { get; set; } = 0;
        /// <summary>
        /// Surname of the entity
        /// </summary>
        public string Surname { get; set; } = string.Empty;
        /// <summary>
        /// Nickname of the entity
        /// </summary>
        public string Nickname { get; set; } = string.Empty;
        /// <summary>
        /// Other related ID of the entity
        /// </summary>
        public string ID { get; set; } = string.Empty;
        /// <summary>
        /// Relations status to other entity/entities
        /// </summary>
        public string RelationshipStatus { get; set; } = string.Empty;
        /// <summary>
        /// Fill basic parameters
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            var pnft = NFT as ProfileNFT;
            Surname = pnft.Surname;
            Nickname = pnft.Nickname;
            RelationshipStatus = pnft.RelationshipStatus;
            Age = pnft.Age;
            ID = pnft.ID;
        }

        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Surname", out var surname))
                Surname = surname;
            if (metadata.TryGetValue("Nickname", out var nickname))
                Nickname = nickname;
            if (metadata.TryGetValue("ID", out var id))
                ID = id;
            if (metadata.TryGetValue("RelationshipStatus", out var relationshipStatus))
                RelationshipStatus = relationshipStatus;
            if (metadata.TryGetValue("Age", out var age))
                Age = Convert.ToInt32(age);
        }
        /// <summary>
        /// Find and parse origin data
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get last data of this NFT
        /// </summary>
        /// <returns></returns>
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
            metadata.Add("Surname", Surname);
            metadata.Add("Nickname", Nickname);
            metadata.Add("Age", Age.ToString());
            if (!string.IsNullOrEmpty(ID))
                metadata.Add("ID", ID);

            return metadata;
        }
    }
}
