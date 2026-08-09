using Household.Application.Commands;
using Household.Application.Dtos;
using Household.Application.Managers;
using Household.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Controllers;

[ApiController]
[Route("api/households")]
public sealed class HouseholdsController(
    IHouseholdManager householdManager,
    IMembershipManager membershipManager,
    IHouseholdQuery householdQuery) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ListMyHouseholds(CancellationToken ct)
    {
        var results = await householdQuery.ListUserHouseholdsAsync(CurrentUserId, ct);
        return Ok(results);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetHousehold(Guid id, CancellationToken ct)
    {
        var result = await householdQuery.GetHouseholdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateHousehold([FromBody] CreateHouseholdRequest request, CancellationToken ct)
    {
        var id = await householdManager.CreateAsync(new CreateHouseholdCommand(
            CurrentUserId, request.Name, request.Description, string.IsNullOrEmpty(request.CurrencyCode) ? "USD" : request.CurrencyCode, request.Timezone), ct);
        return CreatedAtAction(nameof(GetHousehold), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateHousehold(Guid id, [FromBody] UpdateHouseholdRequest request, CancellationToken ct)
    {
        await householdManager.UpdateAsync(new UpdateHouseholdCommand(
            id, CurrentUserId, request.Name, request.Description, request.CurrencyCode, request.Timezone), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(Guid id, [FromBody] TransferOwnershipRequest request, CancellationToken ct)
    {
        await householdManager.TransferOwnershipAsync(new TransferOwnershipCommand(id, CurrentUserId, request.NewOwnerId), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteHousehold(Guid id, CancellationToken ct)
    {
        await householdManager.DeleteAsync(new DeleteHouseholdCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> ListMembers(Guid id, CancellationToken ct)
    {
        var members = await householdQuery.ListMembersAsync(id, ct);
        return Ok(members);
    }

    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, CancellationToken ct)
    {
        await membershipManager.JoinAsync(new JoinHouseholdCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/invite")]
    public async Task<IActionResult> Invite(Guid id, [FromBody] InviteRequest? request, CancellationToken ct)
    {
        var code = await membershipManager.InviteAsync(new InviteMemberCommand(id, CurrentUserId, request?.RecipientEmail), ct);
        return Ok(new { invitationCode = code });
    }

    [HttpPost("accept-invitation")]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request, CancellationToken ct)
    {
        await membershipManager.AcceptInvitationAsync(new AcceptInvitationCommand(request.InvitationCode, CurrentUserId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken ct)
    {
        await membershipManager.LeaveAsync(new LeaveHouseholdCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{membershipId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid membershipId, CancellationToken ct)
    {
        await membershipManager.RemoveAsync(new RemoveMemberCommand(id, membershipId, CurrentUserId), ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/members/{membershipId:guid}/role")]
    public async Task<IActionResult> ChangeMemberRole(Guid id, Guid membershipId, [FromBody] ChangeMemberRoleRequest request, CancellationToken ct)
    {
        await membershipManager.ChangeRoleAsync(new ChangeMemberRoleCommand(id, membershipId, CurrentUserId, request.Role), ct);
        return NoContent();
    }

    // Role-gated: a member may assign their OWN share; assigning another member's requires
    // Owner/Admin. Household authorizes here, then emits an event finance consumes, so this returns
    // 202 — the allocation lands eventually, not before the response.
    [HttpPost("{id:guid}/charges/{chargeId:guid}/allocations")]
    public async Task<IActionResult> AssignAllocation(Guid id, Guid chargeId, [FromBody] AssignAllocationRequest request, CancellationToken ct)
    {
        // Authorization and validation failures surface as domain exceptions and are mapped to
        // ProblemDetails centrally, which is why there is no try/catch here.
        await membershipManager.AssignAllocationAsync(
            new AssignAllocationCommand(id, chargeId, CurrentUserId, request.UserId, request.Amount, request.Currency), ct);
        return Accepted();
    }

    [HttpGet("{id:guid}/activity")]
    public async Task<IActionResult> GetActivity(
        Guid id,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromServices] IActivityFeedQuery activityQuery,
        CancellationToken ct)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 25;
        var result = await activityQuery.ListAsync(id, page, pageSize, ct);
        return Ok(result);
    }
}
