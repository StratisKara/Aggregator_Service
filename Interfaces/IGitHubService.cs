using ApiAggregatorService.Models;

namespace ApiAggregatorService.Interfaces
{
    public interface IGitHubService
    {
        Task<List<GitHubRepo>> SearchReposAsync(string keyword);
    }
}
