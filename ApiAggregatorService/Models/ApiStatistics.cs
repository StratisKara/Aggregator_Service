namespace ApiAggregatorService.Models
{
    public class ApiStatistics
    {
        public int TotalRequests { get; set; }
        public long TotalResponseTime { get; set; }

        // Counters for each category
        public int FastCount { get; set; }
        public int AverageCount { get; set; }
        public int SlowCount { get; set; }

        public double AvergeResponseTime => TotalRequests == 0 ? 0 : (double)TotalResponseTime / TotalRequests;
    }
}
