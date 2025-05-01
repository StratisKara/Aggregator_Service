using ApiAggregatorService.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class GitHubService
{
    private readonly HttpClient _httpClient;

    public GitHubService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

    }

    public async Task<List<GitHubRepo>> SearchReposAsync (string keyword)
    {

        string url = $"https://api.github.com/search/repositories?q={keyword}&sort=stars&order=desc";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();


        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var repos = doc.RootElement.GetProperty("items");

        var result = new List<GitHubRepo>();


        foreach ( var repo in repos.EnumerateArray())
        {
            result.Add(new GitHubRepo
            {
                Name = repo.GetProperty("name").GetString(),
                Owner = repo.GetProperty("owner").GetProperty("login").GetString(),
                Stars = repo.GetProperty("stargazers_count").GetInt32()

            });
        }

        return result;

    }
}