using ContextGraph.Api.DTOs;
using ContextGraph.Api.Models;
using ContextGraph.Api.Repositories;
using ContextGraph.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContextGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphController : ControllerBase
{
    private readonly IGraphRepository _graphRepository;
    private readonly IContextQueryService _contextQueryService;
    public GraphController(IGraphRepository graphRepository, IContextQueryService contextQueryService)
    {
        _graphRepository = graphRepository;
        _contextQueryService = contextQueryService;
    }

    [HttpPost("person-project")]
    public async Task<IActionResult> CreatePersonProject(
        CreatePersonProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PersonId) ||
            string.IsNullOrWhiteSpace(request.PersonName) ||
            string.IsNullOrWhiteSpace(request.ProjectId) ||
            string.IsNullOrWhiteSpace(request.ProjectName))
        {
            return BadRequest("Person and project details are required.");
        }

        try
        {
            var result = await _graphRepository.CreatePersonProjectAsync(
                new Person
                {
                    Id = request.PersonId,
                    Name = request.PersonName
                },
                new Project
                {
                    Id = request.ProjectId,
                    Name = request.ProjectName
                });

            return Ok(new
            {
                person = result.Person,
                project = result.Project,
                relationship = "WORKS_ON"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }

    [HttpGet("person-project/{personId}")]
    public async Task<IActionResult> GetPersonProject(string personId)
    {
        try
        {
            var result = await _graphRepository
                .GetPersonProjectAsync(personId);

            return Ok(new
            {
                person = result.Person,
                project = result.Project,
                relationship = "WORKS_ON"
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new
            {
                message = $"No project relationship found for person '{personId}'."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }

    [HttpPost("project-context")]
    public async Task<IActionResult> CreateProjectContext(
    CreateProjectContextRequest request)
    {
        try
        {
            await _graphRepository.CreateProjectContextAsync(
                request.ProjectId,

                new Meeting
                {
                    Id = request.MeetingId,
                    Title = request.MeetingTitle,
                    Date = request.MeetingDate
                },

                new Decision
                {
                    Id = request.DecisionId,
                    Description = request.DecisionDescription
                },

                new TaskItem
                {
                    Id = request.TaskId,
                    Title = request.TaskTitle,
                    Status = request.TaskStatus
                });

            return Ok(new
            {
                message = "Project context created successfully",
                projectId = request.ProjectId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }

    [HttpGet("project-context/{projectId}")]
    public async Task<IActionResult> GetProjectContext(string projectId)
    {
        try
        {
            var result = await _graphRepository
                .GetProjectContextAsync(projectId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }

    [HttpPost("connect-person-context")]
    public async Task<IActionResult> ConnectPersonContext(
    ConnectPersonContextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PersonId) ||
            string.IsNullOrWhiteSpace(request.MeetingId) ||
            string.IsNullOrWhiteSpace(request.TaskId))
        {
            return BadRequest("PersonId, MeetingId and TaskId are required.");
        }

        try
        {
            await _graphRepository.ConnectPersonToProjectContextAsync(
                request.PersonId,
                request.MeetingId,
                request.TaskId);

            return Ok(new
            {
                message = "Person context connected successfully",
                personId = request.PersonId,
                meetingId = request.MeetingId,
                taskId = request.TaskId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }

    [HttpGet("person-context/{personId}")]
    public async Task<IActionResult> GetPersonContext(string personId)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return BadRequest("Person ID is required.");
        }

        try
        {
            var result = await _graphRepository
                .GetPersonContextAsync(personId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }

    [HttpPost("project-artifacts")]
    public async Task<IActionResult> CreateProjectArtifacts(
    CreateProjectArtifactsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectId) ||
            string.IsNullOrWhiteSpace(request.PersonId))
        {
            return BadRequest("ProjectId and PersonId are required.");
        }

        try
        {
            await _graphRepository.CreateProjectArtifactsAsync(
                request.ProjectId,
                request.PersonId,

                new Document
                {
                    Id = request.DocumentId,
                    Title = request.DocumentTitle,
                    Type = request.DocumentType
                },

                new Email
                {
                    Id = request.EmailId,
                    Subject = request.EmailSubject,
                    Summary = request.EmailSummary
                });

            return Ok(new
            {
                message = "Project artifacts created successfully",
                projectId = request.ProjectId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }

    [HttpGet("project-full-context/{projectId}")]
    public async Task<IActionResult> GetFullProjectContext(
    string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return BadRequest("Project ID is required.");
        }

        try
        {
            var result = await _graphRepository
                .GetFullProjectContextAsync(projectId);

            if (result == null)
            {
                return NotFound(new
                {
                    message = $"Project '{projectId}' was not found."
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }
    [HttpGet("explorer/{projectId}")]
    public async Task<IActionResult> GetGraphExplorer(
    string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return BadRequest("Project ID is required.");
        }

        try
        {
            var result = await _graphRepository
                .GetGraphExplorerAsync(projectId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Database operation failed",
                error = ex.Message
            });
        }
    }
    [HttpPost("query-context")]
    public async Task<IActionResult> QueryContext(
    [FromBody] ContextQueryRequest request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(new
            {
                message = "Query is required."
            });
        }


        var result =
            await _contextQueryService.QueryContextAsync(
                request.Query,
                cancellationToken);


        return Ok(result);
    }
}