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

            if (_cache.TryGetValue(cacheKey, out WeatherInfo cachedWeather))
            {
                _statisticsService.RecordApiStats("Weather", 0);  
                return cachedWeather;
            }

            string apiKey = "6aae3a37a101abb43260d6da5f91a024";
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();


            WeatherInfo weatherInfo = null;

            try
            {

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                weatherInfo = new WeatherInfo
                {
                    City = city,
                    Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
                    Description = root.GetProperty("weather")[0].GetProperty("description").GetString()
                };


                _cache.Set(cacheKey, weatherInfo, TimeSpan.FromMinutes(10));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Weather API call failed: {ex.Message}");

                if (cachedWeather != null)
                {
                    weatherInfo = cachedWeather;
                }
                else 
                { 
                    weatherInfo = new WeatherInfo
                    {
                        City = city,
                        Temperature = 0, 
                        Description = "Unavailable" 
                    };
                }
            }

            stopwatch.Stop();
            _statisticsService.RecordApiStats("Weather", stopwatch.ElapsedMilliseconds);

            return weatherInfo;
        }

    }
}
