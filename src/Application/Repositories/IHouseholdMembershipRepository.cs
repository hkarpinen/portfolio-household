using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;

namespace Household.Application.Repositories;

public interface IHouseholdMembershipRepository
{
    Task<HouseholdMembership?> GetByIdAsync(MembershipId id, CancellationToken ct = default);
    Task<HouseholdMembership?> GetByHouseholdAndUserAsync(HouseholdId householdId, UserId userId, CancellationToken ct = default);
    Task<HouseholdMembership?> GetByInvitationCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<HouseholdMembership>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken ct = default);
    Task<IReadOnlyList<HouseholdMembership>> ListByUserAsync(UserId userId, CancellationToken ct = default);
    Task AddAsync(HouseholdMembership membership, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
