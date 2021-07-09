using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

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
        string NFTOriginTxId { get; set; }
        int UtxoIndex { get; set; }
        DateTime Time { get; set; }
        List<INFT> History { get; set; }
        [JsonIgnore]
        GetTransactionInfoResponse TxDetails { get; set; }
        event EventHandler<GetTransactionInfoResponse> TxDataRefreshed;

        Task FillCommon(INFT nft);
        Task Fill(INFT NFT);
        Task ParseOriginData(IDictionary<string,string> lastmetadata);
        Task LoadHistory();
        Task StopRefreshingData();
        Task StartRefreshingTxData();
        Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "");
    }
}
