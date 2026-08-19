namespace ContextGraph.Api.Models;

public class AIContextResponse
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public ContextQueryResponse Context { get; set; } = new();
}