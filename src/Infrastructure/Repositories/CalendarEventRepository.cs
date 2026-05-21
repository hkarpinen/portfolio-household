using Household.Application.Repositories;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class CalendarEventRepository(HouseholdDbContext db) : ICalendarEventRepository
{
    public Task<HouseholdCalendarEvent?> GetByIdAsync(CalendarEventId id, CancellationToken ct) =>
        db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null, ct);

    public async Task AddAsync(HouseholdCalendarEvent calendarEvent, CancellationToken ct) =>
        await db.CalendarEvents.AddAsync(calendarEvent, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public Task DeleteByHouseholdIdsAsync(IEnumerable<HouseholdId> householdIds, CancellationToken ct)
    {
        var ids = householdIds.Select(h => h.Value).ToList();
        return db.CalendarEvents.Where(c => ids.Contains(c.HouseholdId.Value)).ExecuteDeleteAsync(ct);
    }
}
