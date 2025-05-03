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

            List<NewsArticle> articles = null;

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();


                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var articlesArray = doc.RootElement.GetProperty("articles").EnumerateArray();

                articles = articlesArray.Select(article => new NewsArticle
                {
                    Title = article.GetProperty("title").GetString(),
                    Source = article.GetProperty("source").GetString(),
                    PublishedAt = article.GetProperty("publishedAt").GetDateTime()
                }).ToList();


                _cache.Set(cacheKey, articles, TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"News API call failed: {ex.Message}");

                if (cachedArticles != null)
                {
                    articles = cachedArticles;
                }
                else
                {
                    articles = new List<NewsArticle>
                    {
                        new NewsArticle { Title = "Fallback Article", PublishedAt = DateTime.Now, Source = "Article Fallbacked" }
                    };
                }
            }

            stopwatch.Stop();
            _statisticsService.RecordApiStats("News", stopwatch.ElapsedMilliseconds); 

            return articles;
        }
    }
}
