namespace ContextGraph.Api.DTOs.Context;

public class ProjectContextDto
{
    public ProjectContextProjectDto Project { get; set; } = new();

    public List<ProjectContextPersonDto> People { get; set; } = [];

    public List<ProjectContextMeetingDto> Meetings { get; set; } = [];

    public List<ProjectContextDecisionDto> Decisions { get; set; } = [];

    public List<ProjectContextTaskDto> Tasks { get; set; } = [];

    public List<ProjectContextDocumentDto> Documents { get; set; } = [];

    public List<ProjectContextEmailDto> Emails { get; set; } = [];
}

public class ProjectContextProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ProjectContextPersonDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ProjectContextMeetingDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}

public class ProjectContextDecisionDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ProjectContextTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ProjectContextDocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class ProjectContextEmailDto
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}