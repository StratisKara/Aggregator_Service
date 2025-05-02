using Xunit;
using Moq;
using ApiAggregatorService.Controllers;
using ApiAggregatorService.Models;
using ApiAggregatorService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using ApiAggregatorService.Interfaces;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;

namespace ApiAggregatorService.Tests
{
    public class AggregationControllerTests
    {
        [Fact]
        public async Task Service_ExpectedResults()
        {

            var mockWeather = new Mock<IWeatherService>();
            var mockNews = new Mock<INewsService>();
            var mockGitHub = new Mock<IGitHubService>();

            mockWeather.Setup(w => w.GetWeatherInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(new WeatherInfo
                {
                    Temperature = 25,
                    Description = "Sunny"
                });

            mockNews.Setup(n => n.GetNewsArticlesAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<NewsArticle>
                {
                    new NewsArticle { Title = "Tech News", Source = "Tech Source", PublishedAt = DateTime.Now}
                });

            mockGitHub.Setup(g => g.SearchReposAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<GitHubRepo>
                {
                    new GitHubRepo { Name = "TestRepo", Stars = 100}
                });


            var controller = new AggregationController(mockWeather.Object, mockNews.Object, mockGitHub.Object);

            var result = await controller.GetAggregatedData();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<AggregatedData>(okResult.Value);

            //h

            Assert.NotNull(data);
            Assert.NotNull(data.Weather);
            Assert.NotNull(data.News);
            Assert.NotNull(data.GitHubRepo);

            Assert.Equal("Sunny", data.Weather.Description);
            Assert.Single(data.News);
            Assert.Single(data.GitHubRepo);


        }
    }
}