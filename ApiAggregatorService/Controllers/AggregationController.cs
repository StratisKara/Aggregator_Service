using ApiAggregatorService.Interfaces;
using ApiAggregatorService.Interfaces.ApiAggregatorService.Interfaces;
using ApiAggregatorService.Models;
using ApiAggregatorService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregatorService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AggregationController : ControllerBase
    {

        private readonly IWeatherService _weatherService;
        private readonly INewsService _newsService;
        private readonly IGitHubService _gitHubService;
        private readonly IStatisticsService _statisticsService;

        public AggregationController(IWeatherService weatherService, INewsService newsService, IGitHubService gitHubService, IStatisticsService statisticsService)
        {
            _weatherService = weatherService;
            _newsService = newsService;
            _gitHubService = gitHubService;
            _statisticsService = statisticsService;
        }


        [HttpGet("aggregate")]
        public async Task<IActionResult> GetAggregatedData(
            [FromQuery] string city = "Athens",
            [FromQuery] string newskeyword = "technology",
            [FromQuery] string githubkeyword = "dotnet",
            [FromQuery] DateTime? publishedAfter = null,
            [FromQuery] string sortBy = "date_desc")
        {

            //Track timings
            var swWeather = System.Diagnostics.Stopwatch.StartNew();
            var weatherTask = SafeCall(() => _weatherService.GetWeatherInfoAsync(city));
            swWeather.Stop();
            _statisticsService.RecordApiStats("Weather", swWeather.ElapsedMilliseconds);

            var swNews = System.Diagnostics.Stopwatch.StartNew();
            var newsTask = SafeCall(() => _newsService.GetNewsArticlesAsync(newskeyword));
            swNews.Stop();
            _statisticsService.RecordApiStats("News", swNews.ElapsedMilliseconds);

            var swGitHub = System.Diagnostics.Stopwatch.StartNew();
            var githubTask = SafeCall(() => _gitHubService.SearchReposAsync(githubkeyword));
            swGitHub.Stop();
            _statisticsService.RecordApiStats("GitHub", swGitHub.ElapsedMilliseconds);

            await Task.WhenAll(weatherTask, newsTask, githubTask);

            var weather = weatherTask.Result ?? new WeatherInfo { Temperature = 0, Description = "Unavailable" };
            var newsArticles = newsTask.Result ?? new List<NewsArticle>();
            var githubRepos = githubTask.Result ?? new List<GitHubRepo>();



            if (publishedAfter.HasValue)
            {
                newsArticles = newsArticles
                    .Where(n => n.PublishedAt > publishedAfter.Value)
                    .ToList();
            }

            newsArticles = sortBy switch
            {
                "date_asc" => newsArticles.OrderBy(n => n.PublishedAt).ToList(),
                "date_desc" => newsArticles.OrderByDescending(n => n.PublishedAt).ToList(),
                _ => newsArticles
            };



            var result = new AggregatedData
            {
                Weather = weather,
                News = newsArticles.Take(5),
                GitHubRepo = githubRepos.Take(5)
            };

            return Ok(result);
        }


        [HttpGet("statistics")]
        public IActionResult GetApiStatistics([FromQuery] string apiName = null)
        {
            if (!string.IsNullOrEmpty(apiName))
            {
                var apiStats = _statisticsService.GetApiStats(apiName);
                if (apiStats == null)
                {
                    return NotFound($"No statistics found for API: {apiName}");
                }

                var singleStatsSummary = new Dictionary<string, object>
                {
                    { apiName, apiStats }
                };

                return Ok(singleStatsSummary);
            }

            var weatherStats = _statisticsService.GetApiStats("Weather");
            var newsStats = _statisticsService.GetApiStats("News");
            var gitHubStats = _statisticsService.GetApiStats("GitHub");

            var statsSummary = new
            {
                Weather = weatherStats,
                News = newsStats,
                GitHub = gitHubStats
            };

            return Ok(statsSummary);
        }


        public async Task<T?> SafeCall<T>(Func<Task<T>> apiCall)
        {
            try
            {
                return await apiCall();
            }
            catch(TimeoutException ex)
            {
                Console.WriteLine($"API call timed out: {ex.Message}");
                return default;
            }
            catch(HttpRequestException ex)
            {
                Console.WriteLine($"Network error during API call: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API failed with error: {ex.Message} - {ex.InnerException?.Message}");
                return default;
            }
        }
    }    
}