using ApiAggregatorService.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ApiAggregatorService.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using ApiAggregatorService.Interfaces.ApiAggregatorService.Interfaces;

namespace ApiAggregatorService.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IStatisticsService _statisticsService; 

        public WeatherService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IStatisticsService statisticsService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _cache = cache;
            _statisticsService = statisticsService; 
        }

        public async Task<WeatherInfo> GetWeatherInfoAsync(string city)
        {
            string cacheKey = $"weather_{city}";

            // Only record stats when the data is not cached
            if (_cache.TryGetValue(cacheKey, out WeatherInfo cachedWeather))
            {
                _statisticsService.RecordApiStats("Weather", 0);  // Do not record stats when using cache
                return cachedWeather;
            }

            // If data is not cached, make the API request
            string apiKey = "6aae3a37a101abb43260d6da5f91a024";
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var response = await _httpClient.GetAsync(url);

            stopwatch.Stop();

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var weatherInfo = new WeatherInfo
            {
                City = city,
                Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
                Description = root.GetProperty("weather")[0].GetProperty("description").GetString()
            };

            // Cache the result
            _cache.Set(cacheKey, weatherInfo, TimeSpan.FromMinutes(10));

            // Now record the stats with the actual response time
            _statisticsService.RecordApiStats("Weather", stopwatch.ElapsedMilliseconds);

            return weatherInfo;
        }

    }
}
