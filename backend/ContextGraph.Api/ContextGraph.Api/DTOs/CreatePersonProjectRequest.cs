namespace ContextGraph.Api.DTOs;

public class CreatePersonProjectRequest
{
    public string PersonId { get; set; } = string.Empty;
    public string PersonName { get; set; } = string.Empty;

    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
}