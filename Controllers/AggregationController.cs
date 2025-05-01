using Microsoft.AspNetCore.Mvc;

namespace ApiAggregatorService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AggregationController : ControllerBase
    {

        private readonly WeatherService _weatherService;
        private readonly NewsService _newsService;
        private readonly GitHubService _gitHubService;

        public AggregationController(WeatherService weatherService, NewsService newsService, GitHubService gitHubService)
        {
            _weatherService = weatherService;
            _newsService = newsService;
            _gitHubService = gitHubService;
        }


        [HttpGet("aggregate")]
        public async Task<IActionResult> GetAggregatedData(
            [FromQuery] string city = "Athens",
            [FromQuery] string newskeyword = "technology",
            [FromQuery] string githubkeyword = "dontet")
        {

            var weatherTask = _weatherService.GetWeatherInfoAsync(city);
            var newsTask = _newsService.GetNewsArticlesAsync(newskeyword);
            var githubTask = _gitHubService.SearchReposAsync(githubkeyword);

            await Task.WhenAll(weatherTask, newsTask, githubTask);

            var result = new
            {
                Weather = weatherTask.Result,
                News = newsTask.Result.Take(5),
                GitHubRepo = githubTask.Result.Take(5)
            };

            return Ok(result);
        }
    }    
}