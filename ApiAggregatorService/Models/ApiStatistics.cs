namespace ApiAggregatorService.Models
{
    public class ApiStats
    {
        public int TotalRequests { get; set; } = 0;
        public long TotalResponseTimeMs { get; set; } = 0;
        public int FastRequests { get; set; } = 0; // <100ms
        public int AverageRequests { get; set; } = 0; // 100-200ms
        public int SlowRequests { get; set; } = 0; // >200ms

        public double AverageResponseTime { get; set; } = 0.0;
    }

}
