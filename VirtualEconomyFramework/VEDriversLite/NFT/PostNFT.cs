﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class PostNFT : CommonNFT
    {
        public PostNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Post;
            TypeText = "NFT Post";
        }

        public override async Task Fill(INFT NFT) 
        {
            await FillCommon(NFT);
        }

        public override void ParseSpecific(IDictionary<string, string> metadata)
        {

        }
        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                ParsePrice(lastmetadata);
                await ParseDogeftInfo(lastmetadata);
                ParseSoldInfo(lastmetadata);
                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
            IsLoaded = true;
        }

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);
                ParsePrice(nftData.NFTMetadata);
                ParseSpecific(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
            IsLoaded = true;
        }


        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            var metadata = await GetCommonMetadata();
            return metadata;
        }
    }
}
