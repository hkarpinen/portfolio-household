using Household.Application.Queries;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

internal sealed class CalendarEventQuery(HouseholdDbContext db) : ICalendarEventQuery
{
    public async Task<IReadOnlyList<CalendarEventDto>> ListByHouseholdAsync(
        Guid householdId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var hid = HouseholdId.Create(householdId);
        var events = await db.CalendarEvents
            .AsNoTracking()
            .Where(e => e.HouseholdId == hid && e.DeletedAt == null && e.StartsAt >= from && e.StartsAt <= to)
            .OrderBy(e => e.StartsAt)
            .ToListAsync(ct);

        return events.Select(Map).ToList();
    }

    public async Task<CalendarEventDto?> GetByIdAsync(Guid eventId, CancellationToken ct = default)
    {
        var eid = CalendarEventId.Create(eventId);
        var e = await db.CalendarEvents.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eid && x.DeletedAt == null, ct);
        return e is null ? null : Map(e);
    }

    private static CalendarEventDto Map(HouseholdCalendarEvent e) =>
        new(e.Id.Value, e.HouseholdId.Value, e.Title, e.Description,
            e.StartsAt, e.EndsAt, e.AllDay, e.CreatedByUserId.Value, e.CreatedAt, e.UpdatedAt);
}
