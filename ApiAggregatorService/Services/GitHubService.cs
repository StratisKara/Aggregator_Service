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
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IStatisticsService _statisticsService; 

        public GitHubService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IStatisticsService statisticsService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");
            _cache = cache;
            _statisticsService = statisticsService; 
        }

        public async Task<List<GitHubRepo>> SearchReposAsync(string keyword)
        {
            string cacheKey = $"github_{keyword}";

            if (_cache.TryGetValue(cacheKey, out List<GitHubRepo> cachedRepos))
            {
                _statisticsService.RecordApiStats("GitHub", 0);
                return cachedRepos;
            }

            string url = $"https://api.github.com/search/repositories?q={keyword}&sort=stars&order=desc";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); 

            List<GitHubRepo> repos = null;

            try
            {

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var itemsArray = doc.RootElement.GetProperty("items").EnumerateArray();

                var result = new List<GitHubRepo>();

                repos = itemsArray.Select(item => new GitHubRepo
                {
                    Name = item.GetProperty("name").GetString(),
                    Stars = item.GetProperty("stargazers_count").GetInt32(),
                    Owner = item.GetProperty("owner").GetString()
                }).ToList();

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"GitHub API call failed: {ex.Message}");

                if (cachedRepos != null)
                {
                    repos = cachedRepos;
                }
                else
                {
                    repos = new List<GitHubRepo>
                    {
                        new GitHubRepo { Name = "Fallback Repo", Owner = "Fallback Owner", Stars = 0 }
                    };
                }
            }

            stopwatch.Stop();
            _statisticsService.RecordApiStats("GitHub", stopwatch.ElapsedMilliseconds); 

            return repos;
        }
    }
}
