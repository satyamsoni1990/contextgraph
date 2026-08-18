using ContextGraph.Api.Models;
using ContextGraph.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContextGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly ILLMService _llmService;


    public AIController(
        ILLMService llmService)
    {
        _llmService = llmService;
    }


    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        [FromBody] AIRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
            request.Question))
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
}