using Household.Application.Queries;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

internal sealed class ChoreQuery(HouseholdDbContext db) : IChoreQuery
{
    public async Task<IReadOnlyList<ChoreDto>> ListByHouseholdAsync(Guid householdId, bool activeOnly = true, CancellationToken ct = default)
    {
        var hid = HouseholdId.Create(householdId);
        var chores = await db.Chores
            .AsNoTracking()
            .Where(c => c.HouseholdId == hid && (!activeOnly || c.IsActive))
            .OrderBy(c => c.DueDate)
            .ToListAsync(ct);

        return chores.Select(c => new ChoreDto(
            c.Id.Value, c.HouseholdId.Value, c.Title, c.Description,
            c.AssignedToUserId.HasValue ? c.AssignedToUserId.Value.Value : null,
            c.DueDate, c.RecurrenceFrequency, c.CreatedByUserId.Value,
            c.CreatedAt, c.CompletedAt, c.IsActive)).ToList();
    }

    public async Task<ChoreDto?> GetByIdAsync(Guid choreId, CancellationToken ct = default)
    {
        var cid = ChoreId.Create(choreId);
        var c = await db.Chores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cid, ct);
        if (c is null) return null;
        return new ChoreDto(c.Id.Value, c.HouseholdId.Value, c.Title, c.Description,
            c.AssignedToUserId.HasValue ? c.AssignedToUserId.Value.Value : null,
            c.DueDate, c.RecurrenceFrequency, c.CreatedByUserId.Value,
            c.CreatedAt, c.CompletedAt, c.IsActive);
    }
}
