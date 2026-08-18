using ContextGraph.Api.Configuration;
using ContextGraph.Api.Repositories;
using ContextGraph.Api.Services;
using Neo4j.Driver;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CognoDbService>();


var cognodbUri = Environment.GetEnvironmentVariable("COGNODB_URI");
var cognodbUsername = Environment.GetEnvironmentVariable("COGNODB_USERNAME");
var cognodbPassword = Environment.GetEnvironmentVariable("COGNODB_PASSWORD");

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
        AuthTokens.Basic(cognodbUsername, cognodbPassword)));

builder.Services.AddScoped<IGraphRepository, GraphRepository>();
builder.Services.AddScoped<
    IContextQueryService,
    ContextQueryService>();
builder.Services.AddScoped<ContextAIService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var openRouterOptions =
    builder.Configuration
        .GetSection("OpenRouter")
        .Get<OpenRouterOptions>()
    ?? new OpenRouterOptions();


builder.Services.AddSingleton(
    openRouterOptions);


builder.Services.AddHttpClient<
    ILLMService,
    OpenRouterLLMService>(
    client =>
    {
        client.BaseAddress =
            new Uri(
                openRouterOptions.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(60);
    });
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
