using Household.Application.Dtos;
using Household.Application.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

internal sealed class ActivityFeedQuery : IActivityFeedQuery
{
    private readonly HouseholdDbContext _db;

    public ActivityFeedQuery(HouseholdDbContext db) => _db = db;

    public async Task<ActivityFeedListDto> ListAsync(
        Guid householdId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = _db.ActivityEvents.AsNoTracking()
            .Where(e => e.HouseholdId == householdId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ActivityEventDto(
                e.Id,
                Enum.Parse<ActivityEventType>(e.EventType),
                e.ActorId,
                e.ActorDisplayName,
                e.TargetId,
                e.TargetDescription,
                e.OccurredAt))
            .ToListAsync(ct);

        return new ActivityFeedListDto(items, total);
    }
}
