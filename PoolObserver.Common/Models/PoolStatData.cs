namespace PoolObserver.Common.Models
{
    public class PoolStatData
    {
        public decimal? Time { get; set; }
        public decimal? LastSeen { get; set; }
        public double? ReportedHashrate { get; set; }
        public double? CurrentHashrate { get; set; }
        public int? ValidShares { get; set; }
        public int? InvalidShares { get; set; }
        public int? StaleShares { get; set; }
        public double? AverageHashrate { get; set; }
        public int? ActiveWorkers { get; set; }
        public decimal? Unpaid { get; set; }
        public decimal? Unconfirmed { get; set; }
        public decimal? CoinsPerMin { get; set; }
        public double? UsdPerMin { get; set; }
        public double? BtcPerMin { get; set; }
    }
}
