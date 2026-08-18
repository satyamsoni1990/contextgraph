using ContextGraph.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContextGraph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CognoDbController : ControllerBase
{
    private readonly CognoDbService _cognoDbService;

    public CognoDbController(CognoDbService cognoDbService)
    {
        _cognoDbService = cognoDbService;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            await _cognoDbService.TestConnectionAsync();

            return Ok(new
            {
                status = "Connected",
                database = "CognoDB"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Disconnected",
                error = ex.Message
            });
        }
    }

    [HttpGet("first-query")]
    public async Task<IActionResult> FirstQuery()
    {
        try
        {
            var result = await _cognoDbService.RunFirstQueryAsync();

            return Ok(new
            {
                message = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Query failed",
                error = ex.Message
            });
        }
    }
}