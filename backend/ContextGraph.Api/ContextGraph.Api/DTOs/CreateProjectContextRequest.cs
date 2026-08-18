namespace ContextGraph.Api.DTOs;

public class CreateProjectContextRequest
{
    public string ProjectId { get; set; } = string.Empty;

    public string MeetingId { get; set; } = string.Empty;
    public string MeetingTitle { get; set; } = string.Empty;
    public DateTime MeetingDate { get; set; }

    public string DecisionId { get; set; } = string.Empty;
    public string DecisionDescription { get; set; } = string.Empty;

    public string TaskId { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string TaskStatus { get; set; } = string.Empty;
}