using Household.Application.Commands;
using Household.Application.Repositories;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;

namespace Household.Application.Managers;

public interface IHouseholdManager
{
    Task<Guid> CreateAsync(CreateHouseholdCommand command, CancellationToken ct = default);
    Task UpdateAsync(UpdateHouseholdCommand command, CancellationToken ct = default);
    Task TransferOwnershipAsync(TransferOwnershipCommand command, CancellationToken ct = default);
    Task DeleteAsync(DeleteHouseholdCommand command, CancellationToken ct = default);
}

public sealed class HouseholdManager(
    IHouseholdRepository householdRepo,
    IHouseholdMembershipRepository membershipRepo) : IHouseholdManager
{
    public async Task<Guid> CreateAsync(CreateHouseholdCommand cmd, CancellationToken ct = default)
    {
        var ownerId = UserId.Create(cmd.RequestingUserId);
        var household = Domain.Aggregates.Household.Create(ownerId, cmd.Name, cmd.Description, cmd.CurrencyCode);
        await householdRepo.AddAsync(household, ct);

        // Owner auto-joins as Owner role
        var membership = HouseholdMembership.Create(household.Id, ownerId, HouseholdRole.Owner);
        await membershipRepo.AddAsync(membership, ct);
        await membershipRepo.SaveChangesAsync(ct);

        return household.Id.Value;
    }

    public async Task UpdateAsync(UpdateHouseholdCommand cmd, CancellationToken ct = default)
    {
        var household = await householdRepo.GetByIdAsync(HouseholdId.Create(cmd.HouseholdId), ct)
            ?? throw new KeyNotFoundException($"Household {cmd.HouseholdId} not found.");
        EnsureOwnerOrAdmin(household, cmd.RequestingUserId);
        household.Update(cmd.Name, cmd.Description, cmd.CurrencyCode);
        await householdRepo.SaveChangesAsync(ct);
    }

    public async Task TransferOwnershipAsync(TransferOwnershipCommand cmd, CancellationToken ct = default)
    {
        var household = await householdRepo.GetByIdAsync(HouseholdId.Create(cmd.HouseholdId), ct)
            ?? throw new KeyNotFoundException($"Household {cmd.HouseholdId} not found.");
        EnsureOwner(household, cmd.RequestingUserId);

        var newOwnerMembership = await membershipRepo.GetByHouseholdAndUserAsync(
            household.Id, UserId.Create(cmd.NewOwnerId), ct);
        if (newOwnerMembership is null)
            throw new InvalidOperationException("New owner must be a current member.");

        household.TransferOwnership(UserId.Create(cmd.NewOwnerId));
        newOwnerMembership.ChangeRole(HouseholdRole.Owner);
        await householdRepo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DeleteHouseholdCommand cmd, CancellationToken ct = default)
    {
        var household = await householdRepo.GetByIdAsync(HouseholdId.Create(cmd.HouseholdId), ct)
            ?? throw new KeyNotFoundException($"Household {cmd.HouseholdId} not found.");
        EnsureOwner(household, cmd.RequestingUserId);
        household.Delete();
        await householdRepo.SaveChangesAsync(ct);
    }

    private static void EnsureOwner(Domain.Aggregates.Household household, Guid userId)
    {
        if (household.OwnerId.Value != userId)
            throw new UnauthorizedAccessException("Only the owner can perform this action.");
    }

    private static void EnsureOwnerOrAdmin(Domain.Aggregates.Household household, Guid userId)
    {
        if (household.OwnerId.Value != userId)
            throw new UnauthorizedAccessException("Only the owner or admin can perform this action.");
    }
}
