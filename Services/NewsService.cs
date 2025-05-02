using ApiAggregatorService.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


public class NewsService
{
    private readonly HttpClient _httpClient;

    public NewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        //if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        //{
        //    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ApiAggregatorApp/1.0");
        //}
    }

    public async Task<List<NewsArticle>> GetNewsArticlesAsync(string keyword)
    {

        string apiKey = "6cd59038ee0345278136786b97c36ff1";
        string url = $"https://newsapi.org/v2/everything?q={keyword}&apiKey={apiKey}";

        //var request = new HttpRequestMessage(HttpMethod.Get, url);
        //request.Headers.UserAgent.ParseAdd("ApiAggregatorApp/1.0");

        var response = await _httpClient.GetAsync(url);


        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"News API failed: {response.StatusCode} - {errorContent}");
        }

        response.EnsureSuccessStatusCode();


        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var articles = doc.RootElement.GetProperty("articles");

        var result = new List<NewsArticle>();

        foreach (var article in articles.EnumerateArray())
        {
            result.Add(new NewsArticle
            {
                Title = article.GetProperty("title").GetString(),
                Source = article.GetProperty("source").GetProperty("name").GetString(),
                PublishedAt = article.GetProperty("publishedAt").GetDateTime()
            });
        }

        return result;

    }
}