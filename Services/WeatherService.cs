using ApiAggregatorService.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class WeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<WeatherInfo> GetWeatherInfoAsync(string city)
    {

        string apiKey = "6aae3a37a101abb43260d6da5f91a024";
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new WeatherInfo
        {
            City = city,
            Temperature = root.GetProperty("main").GetProperty("temp").GetDouble(),
            Description = root.GetProperty("weather")[0].GetProperty("description").GetString()
        };
    }
}