# API Aggregation Service

A .NET-based API aggregation service that consolidates data from multiple external APIs (Weather, News, GitHub) and provides a unified endpoint for retrieving aggregated information.

## Features
- Aggregates data from 3 external APIs
- Supports filtering and sorting
- Implements in-memory caching for performance
- Tracks API request statistics
- Gracefully handles API failures with fallback
- Uses asynchronous parallel API calls

---

## Setup & Requirements

- .NET SDK 7.0 or later
- Visual Studio 2022+ or VS Code
- [NewsAPI Key](https://newsapi.org/)
- [OpenWeatherMap Key](https://openweathermap.org/api)

The API will be running on `http://localhost:7237` by default.


## API Endpoints
GET /Aggregation/aggregate
Query Parameters:

- city (default: Athens)
- newskeyword (default: technology)
- githubkeyword (default: dotnet)
- publishedAfter (optional, date)
- sortBy (date_asc | date_desc)

Example: GET /Aggregation/aggregate?city=London&newskeyword=AI&sortBy=date_asc

GET /Aggregation/statistics
Optional parameters: 
- Weather
- News
- GitHub
When parameter is not provided, all statistics are returned.

Returns : 
- Total Requests
- Average Response Time
- Fast/Average/Slow Request Counts


Filtering and Sorting:
- Filter by published date (publishedAfter)
- Sort by date (sortBy=date_asc | sortBy=date_desc)


## Caching
Responses from external APIs are cached for 10 minutes to improve performance and reduce redundant calls.


## Error Handling
The service handles errors gracefully, returning appropriate HTTP status codes and messages for different failure scenarios. 
If an external API fails, the service will return cached data if available.


Example JSON Response :
{
  "weather": {
	"city": "London",
	"temperature": 15,
	"description": "Clear sky"
  },
  "news": [
	{
	  "title": "AI Revolution",
	  "description": "The rise of AI in technology.",
	  "publishedAt": "2023-10-01T12:00:00Z"
	}
  ],
  "github": [
	{
	  "repository": "dotnet/aspnetcore",
	  "stars": 100000,
	  "language": "C#"
	}
  ]
}

## Author 
Karampasis Eftrastios
