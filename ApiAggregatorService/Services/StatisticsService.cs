using ApiAggregatorService.Models;
using ApiAggregatorService.Interfaces;
using System.Collections.Concurrent;
using ApiAggregatorService.Interfaces.ApiAggregatorService.Interfaces;

namespace ApiAggregatorService.Services
{
    public class StatisticsService : IStatisticsService
    {
        
        private static readonly ConcurrentDictionary<string, ApiStats> _apiStats = new ConcurrentDictionary<string, ApiStats>();

        public StatisticsService()
        {
            //_apiStats = new ConcurrentDictionary<string, ApiStats>();
        }

        public void RecordApiStats(string apiName, long responseTime)
        {
            var stats = _apiStats.GetOrAdd(apiName, new ApiStats());

            Console.WriteLine($"Recording stats for {apiName}: {responseTime}ms");

            stats.TotalRequests++;
            stats.AverageResponseTime = ((stats.AverageResponseTime * (stats.TotalRequests - 1)) + responseTime) / stats.TotalRequests;

            Console.WriteLine($"Recorded stats for {apiName}: TotalRequests = {stats.TotalRequests}, AverageResponseTime = {stats.AverageResponseTime} ms");


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

            Console.WriteLine($"Stats recorded for {apiName}: {stats.TotalRequests} requests, Average Response Time: {stats.AverageResponseTime}ms");
        }

        public ApiStats? GetApiStats(string apiName)
        {
            _apiStats.TryGetValue(apiName, out var stats);
            Console.WriteLine($"Getting stats for {apiName}: {stats?.TotalRequests ?? 0} requests");
            return stats;
        }
    }
}
