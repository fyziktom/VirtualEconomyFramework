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
        Payment
    }
    public interface INFT
    {
        string TypeText { get; set; }
        NFTTypes Type { get; set; }
        string Name { get; set; }
        string Author { get; set; }
        string Description { get; set; }
        string Link { get; set; }
        string IconLink { get; set; }
        string ImageLink { get; set; }
        string Tags { get; set; }
        string Utxo { get; set; }
        string SourceTxId { get; set; }
        double Price { get; set; }
        bool PriceActive { get; set; }
        string NFTOriginTxId { get; set; }
        DateTime Time { get; set; }
        List<INFT> History { get; set; }
        GetTransactionInfoResponse TxDetails { get; set; }
        event EventHandler<GetTransactionInfoResponse> TxDataRefreshed;

        Task FillCommon(INFT nft);
        Task Fill(INFT NFT);
        Task ParseOriginData();
        Task LoadHistory();
        Task StopRefreshingData();
        Task StartRefreshingTxData();
    }
}
