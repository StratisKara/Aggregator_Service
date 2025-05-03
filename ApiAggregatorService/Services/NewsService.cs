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
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IStatisticsService _statisticsService; 

        public NewsService(HttpClient httpClient, IMemoryCache cache, IStatisticsService statisticsService)
        {
            _httpClient = httpClient;
            _cache = cache;
            _statisticsService = statisticsService; 
        }

        public async Task<List<NewsArticle>> GetNewsArticlesAsync(string keyword)
        {
            string cacheKey = $"news_{keyword}";

            if (_cache.TryGetValue(cacheKey, out List<NewsArticle> cachedArticles))
            {
                _statisticsService.RecordApiStats("News", 0); 
                return cachedArticles;
            }

            string apiKey = "6cd59038ee0345278136786b97c36ff1";
            string url = $"https://newsapi.org/v2/everything?q={keyword}&apiKey={apiKey}";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); 

            var response = await _httpClient.GetAsync(url);

            stopwatch.Stop();

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

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            _statisticsService.RecordApiStats("News", stopwatch.ElapsedMilliseconds); 

            return result;
        }
    }
}
