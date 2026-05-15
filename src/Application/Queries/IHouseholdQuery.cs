namespace Household.Application.Queries;

public interface IHouseholdQuery
{
    Task<HouseholdDetailDto?> GetHouseholdAsync(Guid householdId, CancellationToken ct = default);
    Task<IReadOnlyList<HouseholdSummaryDto>> ListUserHouseholdsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<MemberDto>> ListMembersAsync(Guid householdId, CancellationToken ct = default);
}
