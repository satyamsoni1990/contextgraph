namespace ContextGraph.Api.DTOs;

public class ConnectPersonContextRequest
{
    public string PersonId { get; set; } = string.Empty;
    public string MeetingId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
}