using Household.Application.Commands;
using Household.Application.Repositories;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;

namespace Household.Application.Managers;

public interface IMembershipManager
{
    Task JoinAsync(JoinHouseholdCommand command, CancellationToken ct = default);
    Task<string> InviteAsync(InviteMemberCommand command, CancellationToken ct = default);
    Task AcceptInvitationAsync(AcceptInvitationCommand command, CancellationToken ct = default);
    Task LeaveAsync(LeaveHouseholdCommand command, CancellationToken ct = default);
    Task RemoveAsync(RemoveMemberCommand command, CancellationToken ct = default);
    Task ChangeRoleAsync(ChangeMemberRoleCommand command, CancellationToken ct = default);
}

public sealed class MembershipManager(
    IHouseholdRepository householdRepo,
    IHouseholdMembershipRepository membershipRepo) : IMembershipManager
{
    public async Task JoinAsync(JoinHouseholdCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        var userId = UserId.Create(cmd.RequestingUserId);

        var existing = await membershipRepo.GetByHouseholdAndUserAsync(householdId, userId, ct);
        if (existing is not null && existing.IsActive)
            throw new InvalidOperationException("User is already a member.");

        var membership = HouseholdMembership.Create(householdId, userId, HouseholdRole.Member);
        await membershipRepo.AddAsync(membership, ct);
        await membershipRepo.SaveChangesAsync(ct);
    }

    public async Task<string> InviteAsync(InviteMemberCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var membership = HouseholdMembership.CreateWithInvitation(householdId, UserId.Create(cmd.RequestingUserId));
        await membershipRepo.AddAsync(membership, ct);
        await membershipRepo.SaveChangesAsync(ct);
        return membership.InvitationCode!;
    }

    public async Task AcceptInvitationAsync(AcceptInvitationCommand cmd, CancellationToken ct = default)
    {
        var membership = await membershipRepo.GetByInvitationCodeAsync(cmd.InvitationCode, ct);
        if (membership is null)
            throw new KeyNotFoundException("Invitation code not found.");

        membership.AcceptInvitation(UserId.Create(cmd.RequestingUserId));
        await membershipRepo.SaveChangesAsync(ct);
    }

    public async Task LeaveAsync(LeaveHouseholdCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        var userId = UserId.Create(cmd.RequestingUserId);
        var membership = await membershipRepo.GetByHouseholdAndUserAsync(householdId, userId, ct);
        if (membership is null)
            throw new KeyNotFoundException("Membership not found.");

        var household = await householdRepo.GetByIdAsync(householdId, ct);
        if (household?.OwnerId.Value == cmd.RequestingUserId)
            throw new InvalidOperationException("Owner cannot leave. Transfer ownership first.");

        membership.Leave();
        await membershipRepo.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(RemoveMemberCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureAdminAsync(householdId, cmd.RequestingUserId, ct);

        var membership = await membershipRepo.GetByIdAsync(MembershipId.Create(cmd.MembershipId), ct);
        if (membership is null)
            throw new KeyNotFoundException("Membership not found.");

        membership.Remove(UserId.Create(cmd.RequestingUserId));
        await membershipRepo.SaveChangesAsync(ct);
    }

    public async Task ChangeRoleAsync(ChangeMemberRoleCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        var household = await householdRepo.GetByIdAsync(householdId, ct);
        if (household is null)
            throw new KeyNotFoundException("Household not found.");

        if (household.OwnerId.Value != cmd.RequestingUserId)
            throw new UnauthorizedAccessException("Only the owner can change member roles.");

        var membership = await membershipRepo.GetByIdAsync(MembershipId.Create(cmd.MembershipId), ct);
        if (membership is null)
            throw new KeyNotFoundException("Membership not found.");

        membership.ChangeRole(cmd.NewRole);
        await membershipRepo.SaveChangesAsync(ct);
    }

    private async Task EnsureMemberAsync(HouseholdId householdId, Guid userId, CancellationToken ct)
    {
        var membership = await membershipRepo.GetByHouseholdAndUserAsync(householdId, UserId.Create(userId), ct);
        if (membership is null || !membership.IsActive)
            throw new UnauthorizedAccessException("User is not a member of this household.");
    }

    private async Task EnsureAdminAsync(HouseholdId householdId, Guid userId, CancellationToken ct)
    {
        var membership = await membershipRepo.GetByHouseholdAndUserAsync(householdId, UserId.Create(userId), ct);
        if (membership is null || !membership.IsActive)
            throw new UnauthorizedAccessException("User is not a member of this household.");
        if (membership.Role is not (HouseholdRole.Admin or HouseholdRole.Owner))
            throw new UnauthorizedAccessException("Admin or Owner role required.");
    }
}
