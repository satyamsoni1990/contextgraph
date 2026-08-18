namespace ContextGraph.Api.DTOs.Context;

public class GraphExplorerDto
{
    public List<GraphNodeDto> Nodes { get; set; } = [];
    public List<GraphRelationshipDto> Relationships { get; set; } = [];
}

public class GraphNodeDto
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}

public class GraphRelationshipDto
{
    public string Source { get; set; } = string.Empty;

    public string Target { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}