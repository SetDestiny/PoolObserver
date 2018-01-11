namespace PoolObserver.Common.Models
{
    public class WorkerData
    {
        public string Worker { get; set; }
        public decimal? Time { get; set; }
        public decimal? LastSeen { get; set; }
        public double? ReportedHashRate { get; set; }
        public double? CurrentHashrate { get; set; }
        public int? ValidShares { get; set; }
        public int? InvalidShares { get; set; }
        public int? StaleShares { get; set; }
        public double? AverageHashRate { get; set; }

    }
}
