using Household.Application.Commands;
using Household.Application.Repositories;
using Household.Domain.ValueObjects;

namespace Household.Application.Managers;

public interface IChoreManager
{
    Task<Guid> CreateAsync(CreateChoreCommand command, CancellationToken ct = default);
    Task AssignAsync(AssignChoreCommand command, CancellationToken ct = default);
    Task CompleteAsync(CompleteChoreCommand command, CancellationToken ct = default);
    Task DeleteAsync(DeleteChoreCommand command, CancellationToken ct = default);
}

public sealed class ChoreManager(
    IChoreRepository choreRepo,
    IHouseholdMembershipRepository membershipRepo) : IChoreManager
{
    public async Task<Guid> CreateAsync(CreateChoreCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var chore = Domain.Aggregates.Chore.Create(
            householdId,
            UserId.Create(cmd.RequestingUserId),
            cmd.Title,
            cmd.Description,
            cmd.DueDate,
            cmd.RecurrenceFrequency);
        await choreRepo.AddAsync(chore, ct);
        await choreRepo.SaveChangesAsync(ct);
        return chore.Id.Value;
    }

    public async Task AssignAsync(AssignChoreCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var chore = await choreRepo.GetByIdAsync(ChoreId.Create(cmd.ChoreId), ct);
        if (chore is null) throw new KeyNotFoundException("Chore not found.");
        chore.Assign(UserId.Create(cmd.AssignToUserId));
        await choreRepo.SaveChangesAsync(ct);
    }

    public async Task CompleteAsync(CompleteChoreCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var chore = await choreRepo.GetByIdAsync(ChoreId.Create(cmd.ChoreId), ct);
        if (chore is null) throw new KeyNotFoundException("Chore not found.");
        chore.Complete(UserId.Create(cmd.RequestingUserId));
        await choreRepo.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(DeleteChoreCommand cmd, CancellationToken ct = default)
    {
        var householdId = HouseholdId.Create(cmd.HouseholdId);
        await EnsureMemberAsync(householdId, cmd.RequestingUserId, ct);

        var chore = await choreRepo.GetByIdAsync(ChoreId.Create(cmd.ChoreId), ct);
        if (chore is null) throw new KeyNotFoundException("Chore not found.");
        chore.Delete();
        await choreRepo.SaveChangesAsync(ct);
    }

    private async Task EnsureMemberAsync(HouseholdId householdId, Guid userId, CancellationToken ct)
    {
        var membership = await membershipRepo.GetByHouseholdAndUserAsync(householdId, UserId.Create(userId), ct);
        if (membership is null || !membership.IsActive)
            throw new UnauthorizedAccessException("User is not a member of this household.");
    }
}
