using ContextGraph.Api.Configuration;
using ContextGraph.Api.Repositories;
using ContextGraph.Api.Services;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

// Render provides the PORT environment variable.
// Locally, use 5122.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5122";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<CognoDbService>();

// --------------------------------------------------
// CognoDB configuration
// --------------------------------------------------

var cognodbUri =
    Environment.GetEnvironmentVariable("COGNODB_URI");

var cognodbUsername =
    Environment.GetEnvironmentVariable("COGNODB_USERNAME");

var cognodbPassword =
    Environment.GetEnvironmentVariable("COGNODB_PASSWORD");

if (string.IsNullOrWhiteSpace(cognodbUri) ||
    string.IsNullOrWhiteSpace(cognodbUsername) ||
    string.IsNullOrWhiteSpace(cognodbPassword))
{
    throw new InvalidOperationException(
        "CognoDB environment variables are not configured.");
}

builder.Services.AddSingleton<IDriver>(_ =>
    GraphDatabase.Driver(
        cognodbUri,
        AuthTokens.Basic(
            cognodbUsername,
            cognodbPassword)));

builder.Services.AddScoped<
    IGraphRepository,
    GraphRepository>();

builder.Services.AddScoped<
    IContextQueryService,
    ContextQueryService>();

builder.Services.AddScoped<ContextAIService>();

// --------------------------------------------------
// CORS
// --------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins(
                "https://satyamsoni1990.github.io")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// --------------------------------------------------
// OpenRouter
// --------------------------------------------------

var openRouterOptions =
    builder.Configuration
        .GetSection("OpenRouter")
        .Get<OpenRouterOptions>()
    ?? new OpenRouterOptions();

builder.Services.AddSingleton(openRouterOptions);

builder.Services.AddHttpClient<
    ILLMService,
    OpenRouterLLMService>(
    client =>
    {
        client.BaseAddress =
            new Uri(openRouterOptions.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(60);
    });

var app = builder.Build();

// --------------------------------------------------
// Swagger
// --------------------------------------------------

app.UseSwagger();
app.UseSwaggerUI();

// --------------------------------------------------
// Health check
// --------------------------------------------------

app.MapGet("/health", () =>
    Results.Ok(new
    {
        status = "healthy",
        service = "ContextGraph.Api"
    }));

// --------------------------------------------------
// HTTP pipeline
// --------------------------------------------------

app.UseCors("AngularClient");

app.UseAuthorization();

app.MapControllers();

app.Run();