using ApiAggregatorService.Models;

namespace ApiAggregatorService.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherInfo> GetWeatherInfoAsync(string city);
    }
}
