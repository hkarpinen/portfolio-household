using Household.Domain.ValueObjects;

namespace Household.Application.Repositories;

public interface IHouseholdRepository
{
    Task<Domain.Aggregates.Household?> GetByIdAsync(HouseholdId id, CancellationToken ct = default);
    Task AddAsync(Domain.Aggregates.Household household, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
