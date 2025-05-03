using ApiAggregatorService.Models;

namespace ApiAggregatorService.Interfaces
{
    namespace ApiAggregatorService.Interfaces
    {
        public interface IStatisticsService
        {
            void RecordApiStats(string apiName, long responseTime);  
            ApiStats GetApiStats(string apiName);  
        }
    }

}
