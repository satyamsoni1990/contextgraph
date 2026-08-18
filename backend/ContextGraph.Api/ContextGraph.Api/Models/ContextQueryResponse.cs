namespace ContextGraph.Api.Models;

public class ContextQueryResponse
{
    public string Query { get; set; } = string.Empty;

    public string Intent { get; set; } = string.Empty;

    public ContextPerson? Person { get; set; }

    public List<ContextConnection> Connections { get; set; } = new();
}


public class ContextPerson
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}


public class ContextConnection
{
    public string Relationship { get; set; } = string.Empty;

    public string NodeId { get; set; } = string.Empty;

    public string NodeName { get; set; } = string.Empty;

    public string NodeType { get; set; } = string.Empty;
}