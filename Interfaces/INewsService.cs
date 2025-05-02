using ApiAggregatorService.Models;

namespace ApiAggregatorService.Interfaces
{
    public interface INewsService
    {
        Task<List<NewsArticle>> GetNewsArticlesAsync(string keyword);
    }
}
