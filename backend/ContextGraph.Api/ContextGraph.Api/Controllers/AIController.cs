using ContextGraph.Api.Models;
using ContextGraph.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContextGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly ILLMService _llmService;
    private readonly ContextAIService _contextAIService;

    public AIController(
        ILLMService llmService,
        ContextAIService contextAIService)
    {
        _llmService = llmService;
        _contextAIService = contextAIService;
    }

    // ==========================================
    // GENERAL AI
    // ==========================================

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        [FromBody] AIRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new
            {
                message = "Question is required."
            });
        }

        var answer =
            await _llmService.GenerateAnswerAsync(
                request.Question,
                cancellationToken);

        return Ok(new
        {
            question = request.Question,
            answer
        });
    }

    // ==========================================
    // GRAPH + AI
    // ==========================================

    [HttpPost("context")]
    public async Task<IActionResult> AskWithContext(
        [FromBody] AIRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new
            {
                message = "Question is required."
            });
        }

        var answer =
            await _contextAIService.AskWithContextAsync(
                request.Question,
                cancellationToken);

        return Ok(new
        {
            question = request.Question,
            answer
        });
    }
}