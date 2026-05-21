using Household.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Client.Controllers;

[ApiController]
[Route("api/households/demo")]
public sealed class DemoController(IDemoQuery demoQuery) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken ct)
    {
        var ready = await demoQuery.IsDemoReadyAsync(CurrentUserId, ct);
        return Ok(new { ready });
    }
}
