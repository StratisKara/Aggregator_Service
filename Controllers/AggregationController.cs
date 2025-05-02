using ApiAggregatorService.Interfaces;
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

        public AggregationController(IWeatherService weatherService, INewsService newsService, IGitHubService gitHubService)
        {
            _weatherService = weatherService;
            _newsService = newsService;
            _gitHubService = gitHubService;
        }


        [HttpGet("aggregate")]
        public async Task<IActionResult> GetAggregatedData(
            [FromQuery] string city = "Athens",
            [FromQuery] string newskeyword = "technology",
            [FromQuery] string githubkeyword = "dotnet",
            [FromQuery] DateTime? publishedAfter = null,
            [FromQuery] string sortBy = "date_desc")
        {

            var weatherTask = SafeCall(() => _weatherService.GetWeatherInfoAsync(city));
            var newsTask = SafeCall(() => _newsService.GetNewsArticlesAsync(newskeyword));
            var githubTask = SafeCall(() => _gitHubService.SearchReposAsync(githubkeyword));

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
                "date_desc" => newsArticles.OrderBy(n => n.PublishedAt).ToList(),
                _ => newsArticles
            };



            var result = new
            {
                Weather = weather,
                News = newsArticles.Take(5),
                GitHubRepo = githubRepos.Take(5)
            };

            return Ok(result);
        }

        public async Task<T?> SafeCall<T>(Func<Task<T>> apiCall)
        {
            try
            {
                return await apiCall();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"API call failed with message: {ex.Message}");
                return default;
            }
        }
    }    
}