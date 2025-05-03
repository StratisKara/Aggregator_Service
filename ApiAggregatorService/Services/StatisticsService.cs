using ApiAggregatorService.Models;
using ApiAggregatorService.Interfaces;
using System.Collections.Concurrent;
using ApiAggregatorService.Interfaces.ApiAggregatorService.Interfaces;

namespace ApiAggregatorService.Services
{
    public class StatisticsService : IStatisticsService
    {
        
        private readonly ConcurrentDictionary<string, ApiStats> _apiStats;

        public StatisticsService()
        {
            _apiStats = new ConcurrentDictionary<string, ApiStats>();
        }

        public void RecordApiStats(string apiName, long responseTime)
        {
            var stats = _apiStats.GetOrAdd(apiName, new ApiStats());

            stats.TotalRequests++;
            stats.AverageResponseTime = ((stats.AverageResponseTime * (stats.TotalRequests - 1)) + responseTime) / stats.TotalRequests;

            
            if (responseTime < 100)
            {
                stats.FastRequests++;
            }
            else if (responseTime >= 100 && responseTime <= 200)
            {
                stats.AverageRequests++;
            }
            else
            {
                stats.SlowRequests++;
            }
        }

        public ApiStats? GetApiStats(string apiName)
        {
            _apiStats.TryGetValue(apiName, out var stats);
            return stats;
        }
    }
}
