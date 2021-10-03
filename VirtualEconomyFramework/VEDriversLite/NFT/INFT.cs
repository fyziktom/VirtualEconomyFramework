using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.NFT
{
    public enum NFTTypes
    {
        Image,
        Post,
        Profile,
        Music,
        YouTube,
        Spotify,
        Payment,
        Message,
        Ticket,
        Event,
        Receipt,
        CoruzantArticle = 101,
        CoruzantPremiumArticle = 102,
        CoruzantPodcast = 103,
        CoruzantPremiumPodcast = 104,
        CoruzantProfile = 105
    }
    public interface INFT
    {
        string TypeText { get; set; }
        NFTTypes Type { get; set; }
        string Name { get; set; }
        string Author { get; set; }
        string Description { get; set; }
        string Text { get; set; }
        string Link { get; set; }
        string IconLink { get; set; }
        string ImageLink { get; set; }
        string Tags { get; set; }
        List<string> TagsList { get; set; }
        string Utxo { get; set; }
        string TokenId { get; set; }
        string SourceTxId { get; set; }
        double Price { get; set; }
        bool PriceActive { get; set; }
        double DogePrice { get; set; }
        bool DogePriceActive { get; set; }
        string DogeAddress { get; set; }
        DogeftInfo DogeftInfo { get; set; }
        string NFTOriginTxId { get; set; }
        int UtxoIndex { get; set; }
        string ShortHash { get; }
        bool IsLoaded { get; set; }
        NFTSoldInfo SoldInfo { get; set; }
        DateTime Time { get; set; }
        List<INFT> History { get; set; }
        [JsonIgnore]
        GetTransactionInfoResponse TxDetails { get; set; }
        event EventHandler<GetTransactionInfoResponse> TxDataRefreshed;

        bool IsSpendable();
        Task FillCommon(INFT nft);
        Task Fill(INFT NFT);
        Task LoadLastData(IDictionary<string, string> metadata);
        Task ParseOriginData(IDictionary<string,string> lastmetadata);
        void ParseSoldInfo(IDictionary<string, string> meta);
        void ParsePrice(IDictionary<string, string> meta);
        void ParseSpecific(IDictionary<string, string> meta);
        Task ParseDogeftInfo(IDictionary<string, string> meta);
        Task ClearSoldInfo();
        Task LoadHistory();
        Task ClearPrices();
        Task StopRefreshingData();
        Task StartRefreshingTxData(int interval = 5000);
        Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "");
    }
}
