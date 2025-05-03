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

            var response = await _httpClient.GetAsync(url);

            stopwatch.Stop(); 

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub API failed: {response.StatusCode} - {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var repos = doc.RootElement.GetProperty("items");

            var result = new List<GitHubRepo>();

            foreach (var repo in repos.EnumerateArray())
            {
                result.Add(new GitHubRepo
                {
                    Name = repo.GetProperty("name").GetString(),
                    Owner = repo.GetProperty("owner").GetProperty("login").GetString(),
                    Stars = repo.GetProperty("stargazers_count").GetInt32()
                });
            }

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            _statisticsService.RecordApiStats("GitHub", stopwatch.ElapsedMilliseconds); 

            return result;
        }
    }
}
