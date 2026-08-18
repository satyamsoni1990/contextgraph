using System.Text;
using System.Text.Json;
using ContextGraph.Api.Models;

namespace ContextGraph.Api.Services;

public class ContextAIService
{
    private readonly IContextQueryService _contextQueryService;
    private readonly ILLMService _llmService;

    public ContextAIService(
        IContextQueryService contextQueryService,
        ILLMService llmService)
    {
        _contextQueryService = contextQueryService;
        _llmService = llmService;
    }

    public async Task<string> AskWithContextAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        // Step 1:
        // Retrieve verified context from CognoDB
        var context =
            await _contextQueryService.QueryContextAsync(
                question,
                cancellationToken);

        // Step 2:
        // Convert graph context into JSON
        var contextJson =
            JsonSerializer.Serialize(
                context,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        // Step 3:
        // Build a grounded prompt
        var prompt = BuildPrompt(
            question,
            contextJson);

        // Step 4:
        // Send graph context + question to LLM
        return await _llmService.GenerateAnswerAsync(
            prompt,
            cancellationToken);
    }

    private static string BuildPrompt(
        string question,
        string contextJson)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine(
            "You are ContextGraph AI, an assistant that answers questions about work context.");

        prompt.AppendLine();

        prompt.AppendLine(
            "IMPORTANT RULES:");

        prompt.AppendLine(
            "1. Answer only using the supplied graph context.");

        prompt.AppendLine(
            "2. Do not invent people, projects, tasks, meetings, decisions, documents or emails.");

        prompt.AppendLine(
            "3. If the context does not contain enough information, say that the available context is insufficient.");

        prompt.AppendLine(
            "4. Give a concise business-friendly answer.");

        prompt.AppendLine();

        prompt.AppendLine(
            "USER QUESTION:");

        prompt.AppendLine(question);

        prompt.AppendLine();

        prompt.AppendLine(
            "GRAPH CONTEXT:");

        prompt.AppendLine(contextJson);

        prompt.AppendLine();

        prompt.AppendLine(
            "ANSWER:");

        return prompt.ToString();
    }
}