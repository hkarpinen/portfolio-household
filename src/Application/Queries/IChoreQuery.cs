namespace Household.Application.Queries;

public interface IChoreQuery
{
    Task<IReadOnlyList<ChoreDto>> ListByHouseholdAsync(Guid householdId, bool activeOnly = true, CancellationToken ct = default);
    Task<ChoreDto?> GetByIdAsync(Guid choreId, CancellationToken ct = default);
}
