using Household.Application.Dtos;

namespace Household.Application.Queries;

public interface IActivityFeedQuery
{
    Task<ActivityFeedListDto> ListAsync(Guid householdId, int page, int pageSize, CancellationToken ct);
}
