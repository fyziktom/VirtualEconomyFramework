namespace VEFramework.BlockchainIndexerServer
{
    public class ServerStatusDto
    {
        public double CountOfBlocks { get; set; } = -1;
        public double CountOfIndexedBlocks { get; set; } = -1;
        public double AverageTimeToIndexBlockInMilliseconds { get; set; } = 1;
        public double CountOfTransactions { get; set; } = -1;
        public double CountOfUtxos { get; set; } = -1;
        public double CountOfUsedUtxos { get; set; } = -1;
        public double CountOfAddresses { get; set; } = -1;
        public double ActualOldestLoadedBlock { get; set; } = -1;
        public double LatestLoadedBlock { get; set; } = -1;
        public double OldestBlockToLoad { get; set; } = -1;
        public double RestToLoad { get => ActualOldestLoadedBlock - OldestBlockToLoad; }
        public double RestToLoadInPercentage
        {
            get
            {
                var total = RestToLoad + (LatestLoadedBlock - ActualOldestLoadedBlock);
                if (total != 0)
                {
                    var r = 100 * (RestToLoad / total);
                    if (r > -100 && r < 100)
                        return Math.Abs(r);
                    else
                        return 100.0;
                }
                else
                    return 100.0;
            }
        }
        public double ActualLoadingStatus
        {
            get
            {
                var rest = RestToLoadInPercentage;
                if (rest >= 0 && rest <= 100)
                    return 100 - rest;
                else
                    return 100;
            }
        }
        public double EstimatedTimeToFinishInMinutes
        {
            get
            {
                var r = AverageTimeToIndexBlockInMilliseconds * RestToLoad;
                return Convert.ToDouble(r / 6000);
            }
        }
        public double EstimatedTimeToFinishInHours { get => EstimatedTimeToFinishInMinutes / 60; }
    }
}
