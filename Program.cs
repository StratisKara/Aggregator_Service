using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Register your custom services
builder.Services.AddScoped<WeatherService>();
//builder.Services.AddScoped<NewsService>();
builder.Services.AddHttpClient<NewsService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApiAggregatorApp/1.0");
});
builder.Services.AddScoped<GitHubService>();

// Add controllers
builder.Services.AddControllers();

// Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Aggregator", Version = "v1" });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Static files middleware (built-in to ASP.NET Core 7)
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
