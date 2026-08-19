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

    public async Task<AIContextResponse> AskWithContextAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        // 1. Get verified context from CognoDB
        var context =
            await _contextQueryService.QueryContextAsync(
                question,
                cancellationToken);

        // 2. Convert graph context to JSON
        var contextJson =
            JsonSerializer.Serialize(
                context,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        // 3. Build grounded prompt
        var prompt = BuildPrompt(
            question,
            contextJson);

        // 4. Ask LLM
        var answer =
            await _llmService.GenerateAnswerAsync(
                prompt,
                cancellationToken);

        // 5. Return both answer and source context
        return new AIContextResponse
        {
            Question = question,
            Answer = answer,
            Context = context
        };
    }

    private static string BuildPrompt(
        string question,
        string contextJson)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine(
            "You are ContextGraph AI, an assistant that answers questions about work context.");

        prompt.AppendLine();

        prompt.AppendLine("IMPORTANT RULES:");

        prompt.AppendLine(
            "1. Answer only using the supplied graph context.");

        prompt.AppendLine(
            "2. Do not invent people, projects, tasks, meetings, decisions, documents or emails.");

        prompt.AppendLine(
            "3. If the context does not contain enough information, say that the available context is insufficient.");

        prompt.AppendLine(
            "4. Give a concise business-friendly answer.");

        prompt.AppendLine(
            "5. When useful, mention the relevant relationships from the graph.");

        prompt.AppendLine();

        prompt.AppendLine("USER QUESTION:");

        prompt.AppendLine(question);

        prompt.AppendLine();

        prompt.AppendLine("GRAPH CONTEXT:");

        prompt.AppendLine(contextJson);

        prompt.AppendLine();

        prompt.AppendLine("ANSWER:");

        return prompt.ToString();
    }
}