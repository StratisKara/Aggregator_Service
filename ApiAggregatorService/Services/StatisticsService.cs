using ApiAggregatorService.Interfaces;
using System.Collections.Concurrent;
using ApiAggregatorService.Models;

namespace ApiAggregatorService.Services
{
    public class StatisticsService : IStatisticsService
    {

        private readonly ConcurrentDictionary<string, ApiStatistics> _apiStats = new();

        public void LogRequest(string apiName, long responseTime)
        {

            var apiStats = _apiStats.GetOrAdd(apiName, new ApiStatistics());

            lock (apiStats)
            {
                apiStats.TotalRequests++;
                apiStats.TotalResponseTime += responseTime;

                if (responseTime < 100)
                {
                    apiStats.FastCount++;
                }
                else if (responseTime <= 200)
                {
                    apiStats.AverageCount++;
                }
                else
                {
                    apiStats.SlowCount++;
                }
            }
        }

        public Dictionary<string, ApiStatistics> GetStatistics()
        {
            return _apiStats.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
