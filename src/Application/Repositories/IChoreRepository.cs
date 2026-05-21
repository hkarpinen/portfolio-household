using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;

namespace Household.Application.Repositories;

public interface IChoreRepository
{
    Task<Chore?> GetByIdAsync(ChoreId id, CancellationToken ct = default);
    Task AddAsync(Chore chore, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task DeleteByHouseholdIdsAsync(IEnumerable<HouseholdId> householdIds, CancellationToken ct = default);
}
