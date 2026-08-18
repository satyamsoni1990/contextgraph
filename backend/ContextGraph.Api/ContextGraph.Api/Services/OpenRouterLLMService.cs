using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ContextGraph.Api.Configuration;

namespace ContextGraph.Api.Services;

public class OpenRouterLLMService : ILLMService
{
    private readonly HttpClient _httpClient;

    private readonly OpenRouterOptions _options;


    public OpenRouterLLMService(
        HttpClient httpClient,
        OpenRouterOptions options)
    {
        _httpClient = httpClient;

        _options = options;
    }


    public async Task<string> GenerateAnswerAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
            _options.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenRouter API key is not configured.");
        }


        var requestBody = new
        {
            model = _options.Model,

            messages = new[]
            {
                new
                {
                    role = "system",

                    content =
                        "You are a helpful AI assistant. " +
                        "Give concise and accurate answers."
                },

                new
                {
                    role = "user",

                    content = question
                }
            },

            temperature = 0.2,

            max_tokens = 500
        };


        var json =
            JsonSerializer.Serialize(requestBody);


        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "chat/completions");


        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey);


        request.Headers.Add(
            "HTTP-Referer",
            "http://localhost:4200");


        request.Headers.Add(
            "X-Title",
            "ContextGraph");


        request.Content =
            new StringContent(
                json,
                Encoding.UTF8,
                "application/json");


        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);


        var responseContent =
            await response.Content.ReadAsStringAsync(
                cancellationToken);


        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenRouter request failed. " +
                $"Status: {response.StatusCode}. " +
                $"Response: {responseContent}");
        }


        using var document =
            JsonDocument.Parse(responseContent);


        var answer =
            document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();


        return answer ?? string.Empty;
    }
}