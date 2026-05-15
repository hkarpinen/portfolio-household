using Household.Application.Commands;
using Household.Application.Managers;
using Household.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Client.Controllers;

[ApiController]
[Route("api/households")]
public sealed class HouseholdsController(
    IHouseholdManager householdManager,
    IMembershipManager membershipManager,
    IHouseholdQuery householdQuery) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/households
    [HttpGet]
    public async Task<IActionResult> ListMyHouseholds(CancellationToken ct)
    {
        var results = await householdQuery.ListUserHouseholdsAsync(CurrentUserId, ct);
        return Ok(results);
    }

    // GET /api/households/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetHousehold(Guid id, CancellationToken ct)
    {
        var result = await householdQuery.GetHouseholdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST /api/households
    [HttpPost]
    public async Task<IActionResult> CreateHousehold([FromBody] CreateHouseholdRequest request, CancellationToken ct)
    {
        var id = await householdManager.CreateAsync(new CreateHouseholdCommand(
            CurrentUserId, request.Name, request.Description, string.IsNullOrEmpty(request.CurrencyCode) ? "USD" : request.CurrencyCode), ct);
        return CreatedAtAction(nameof(GetHousehold), new { id }, new { id });
    }

    // PUT /api/households/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateHousehold(Guid id, [FromBody] UpdateHouseholdRequest request, CancellationToken ct)
    {
        await householdManager.UpdateAsync(new UpdateHouseholdCommand(
            id, CurrentUserId, request.Name, request.Description, request.CurrencyCode), ct);
        return NoContent();
    }

    // POST /api/households/{id}/transfer-ownership
    [HttpPost("{id:guid}/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(Guid id, [FromBody] TransferOwnershipRequest request, CancellationToken ct)
    {
        await householdManager.TransferOwnershipAsync(new TransferOwnershipCommand(id, CurrentUserId, request.NewOwnerId), ct);
        return NoContent();
    }

    // DELETE /api/households/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteHousehold(Guid id, CancellationToken ct)
    {
        await householdManager.DeleteAsync(new DeleteHouseholdCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    // GET /api/households/{id}/members
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> ListMembers(Guid id, CancellationToken ct)
    {
        var members = await householdQuery.ListMembersAsync(id, ct);
        return Ok(members);
    }

    // POST /api/households/{id}/join
    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, CancellationToken ct)
    {
        await membershipManager.JoinAsync(new JoinHouseholdCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    // POST /api/households/{id}/invite
    [HttpPost("{id:guid}/invite")]
    public async Task<IActionResult> Invite(Guid id, CancellationToken ct)
    {
        var code = await membershipManager.InviteAsync(new InviteMemberCommand(id, CurrentUserId), ct);
        return Ok(new { invitationCode = code });
    }

    // POST /api/households/accept-invitation
    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken ct)
    {
        await membershipManager.AcceptInvitationAsync(new AcceptInvitationCommand(request.InvitationCode, CurrentUserId), ct);
        return NoContent();
    }

    // POST /api/households/{id}/leave
    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken ct)
    {
        await membershipManager.LeaveAsync(new LeaveHouseholdCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    // DELETE /api/households/{id}/members/{membershipId}
    [HttpDelete("{id:guid}/members/{membershipId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid membershipId, CancellationToken ct)
    {
        await membershipManager.RemoveAsync(new RemoveMemberCommand(id, membershipId, CurrentUserId), ct);
        return NoContent();
    }

    // PUT /api/households/{id}/members/{membershipId}/role
    [HttpPut("{id:guid}/members/{membershipId:guid}/role")]
    public async Task<IActionResult> ChangeMemberRole(Guid id, Guid membershipId, [FromBody] ChangeMemberRoleRequest request, CancellationToken ct)
    {
        await membershipManager.ChangeRoleAsync(new ChangeMemberRoleCommand(id, membershipId, CurrentUserId, request.Role), ct);
        return NoContent();
    }
}

// Request models
public sealed record CreateHouseholdRequest(string Name, string? Description, string CurrencyCode);
public sealed record UpdateHouseholdRequest(string Name, string? Description, string CurrencyCode);
public sealed record TransferOwnershipRequest(Guid NewOwnerId);
public sealed record AcceptInvitationRequest(string InvitationCode);
public sealed record ChangeMemberRoleRequest(Household.Domain.ValueObjects.HouseholdRole Role);
