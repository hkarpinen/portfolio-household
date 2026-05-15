using Household.Application.Commands;
using Household.Application.Managers;
using Household.Application.Queries;
using Household.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Client.Controllers;

[ApiController]
[Route("api/households/{householdId:guid}/chores")]
public sealed class ChoresController(
    IChoreManager choreManager,
    IChoreQuery choreQuery) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> List(Guid householdId, [FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var chores = await choreQuery.ListByHouseholdAsync(householdId, activeOnly, ct);
        return Ok(chores);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid householdId, Guid id, CancellationToken ct)
    {
        var chore = await choreQuery.GetByIdAsync(id, ct);
        return chore is null ? NotFound() : Ok(chore);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid householdId, [FromBody] CreateChoreRequest request, CancellationToken ct)
    {
        var id = await choreManager.CreateAsync(new CreateChoreCommand(
            householdId, CurrentUserId, request.Title, request.Description, request.DueDate, request.RecurrenceFrequency), ct);
        return CreatedAtAction(nameof(Get), new { householdId, id }, new { id });
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid householdId, Guid id, [FromBody] AssignChoreRequest request, CancellationToken ct)
    {
        await choreManager.AssignAsync(new AssignChoreCommand(id, householdId, CurrentUserId, request.AssignToUserId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid householdId, Guid id, CancellationToken ct)
    {
        await choreManager.CompleteAsync(new CompleteChoreCommand(id, householdId, CurrentUserId), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid householdId, Guid id, CancellationToken ct)
    {
        await choreManager.DeleteAsync(new DeleteChoreCommand(id, householdId, CurrentUserId), ct);
        return NoContent();
    }
}

public sealed record CreateChoreRequest(string Title, string? Description, DateTime? DueDate, RecurrenceFrequency? RecurrenceFrequency);
public sealed record AssignChoreRequest(Guid AssignToUserId);
