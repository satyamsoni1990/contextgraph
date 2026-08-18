namespace ContextGraph.Api.Services;

public interface ILLMService
{
    Task<string> GenerateAnswerAsync(
        string question,
        CancellationToken cancellationToken = default);
}