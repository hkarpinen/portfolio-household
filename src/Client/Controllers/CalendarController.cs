using Household.Application.Commands;
using Household.Application.Managers;
using Household.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Client.Controllers;

[ApiController]
[Route("api/households/{householdId:guid}/calendar")]
public sealed class CalendarController(
    ICalendarEventManager calendarManager,
    ICalendarEventQuery calendarQuery) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> List(
        Guid householdId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var events = await calendarQuery.ListByHouseholdAsync(householdId, from, to, ct);
        return Ok(events);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid householdId, Guid id, CancellationToken ct)
    {
        var ev = await calendarQuery.GetByIdAsync(id, ct);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid householdId, [FromBody] CreateCalendarEventRequest request, CancellationToken ct)
    {
        var id = await calendarManager.CreateAsync(new CreateCalendarEventCommand(
            householdId, CurrentUserId, request.Title, request.Description,
            request.StartsAt, request.EndsAt, request.AllDay), ct);
        return CreatedAtAction(nameof(Get), new { householdId, id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid householdId, Guid id, [FromBody] UpdateCalendarEventRequest request, CancellationToken ct)
    {
        await calendarManager.UpdateAsync(new UpdateCalendarEventCommand(
            id, householdId, CurrentUserId, request.Title, request.Description,
            request.StartsAt, request.EndsAt, request.AllDay), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid householdId, Guid id, CancellationToken ct)
    {
        await calendarManager.DeleteAsync(new DeleteCalendarEventCommand(id, householdId, CurrentUserId), ct);
        return NoContent();
    }
}

public sealed record CreateCalendarEventRequest(string Title, string? Description, DateTime StartsAt, DateTime? EndsAt, bool AllDay);
public sealed record UpdateCalendarEventRequest(string Title, string? Description, DateTime StartsAt, DateTime? EndsAt, bool AllDay);
