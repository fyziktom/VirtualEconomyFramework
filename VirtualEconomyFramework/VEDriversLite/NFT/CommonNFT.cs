using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT
{
    public abstract class CommonNFT : INFT
    {
        public string TypeText { get; set; } = string.Empty;
        public NFTTypes Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string IconLink { get; set; } = string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Utxo { get; set; } = string.Empty;
        public string SourceTxId { get; set; } = string.Empty;
        public string NFTOriginTxId { get; set; } = string.Empty;
        public double Price { get; set; } = 0.0;
        public bool PriceActive { get; set; } = false;
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public List<INFT> History { get; set; } = new List<INFT>();

        public GetTransactionInfoResponse TxDetails { get; set; } = new GetTransactionInfoResponse();
        private System.Threading.Timer txdetailsTimer;

        public event EventHandler<GetTransactionInfoResponse> TxDataRefreshed;

        public abstract Task ParseOriginData();

        public async Task LoadHistory()
        {
            History = await NFTHelpers.LoadNFTsHistory(Utxo);
        }

        public async Task StopRefreshingData()
        {
            if (txdetailsTimer != null)
                txdetailsTimer.Dispose();
        }
        
        public async Task StartRefreshingTxData()
        {
            TxDetails.Confirmations = 0;
            TxDetails.Time = 0;

            if (txdetailsTimer != null)
            {
                txdetailsTimer.Dispose();
            }

            try
            {
                TxDetails = await NeblioTransactionHelpers.GetTransactionInfo(Utxo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot read tx details. " + ex.Message);
            }

            txdetailsTimer = new System.Threading.Timer(async (object stateInfo) =>
            {
                if (!string.IsNullOrEmpty(Utxo))
                {
                    try
                    {
                        TxDetails = await NeblioTransactionHelpers.GetTransactionInfo(Utxo);
                        TxDataRefreshed?.Invoke(this, TxDetails);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot read tx details. " + ex.Message);
                    }
                }

            }, new System.Threading.AutoResetEvent(false), 5000, 5000);
        }
    }
}
