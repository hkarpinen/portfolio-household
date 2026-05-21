using Household.Application.Queries;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

internal sealed class DemoQuery(HouseholdDbContext db) : IDemoQuery
{
    public async Task<bool> IsDemoReadyAsync(Guid userId, CancellationToken ct = default)
    {
        var projection = await db.UserProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        return projection?.DemoSeedCompletedAt is not null;
    }
}
