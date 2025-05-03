using ApiAggregatorService.Models;

namespace ApiAggregatorService.Interfaces
{
    public interface IStatisticsService
    {
        void LogRequest(string apiName, long responseTime);
        Dictionary<string, ApiStatistics> GetStatistics();
    }
}
