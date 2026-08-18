namespace ContextGraph.Api.DTOs;

public class CreateProjectArtifactsRequest
{
    public string ProjectId { get; set; } = string.Empty;

    public string PersonId { get; set; } = string.Empty;

    public string DocumentId { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;

    public string EmailId { get; set; } = string.Empty;
    public string EmailSubject { get; set; } = string.Empty;
    public string EmailSummary { get; set; } = string.Empty;
}