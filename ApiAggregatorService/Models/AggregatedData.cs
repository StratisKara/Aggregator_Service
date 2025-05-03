namespace ApiAggregatorService.Models
{
    public class AggregatedData
    {
        public WeatherInfo Weather { get; set; }
        public IEnumerable<NewsArticle> News { get; set; }
        public IEnumerable<GitHubRepo> GitHubRepo { get; set; }
    }
}
