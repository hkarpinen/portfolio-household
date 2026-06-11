using Household.Application.Queries;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

internal sealed class CalendarEventQuery(HouseholdDbContext db) : ICalendarEventQuery
{
    /// <summary>
    /// Member-source entries: returned by `StartsAt` falling in [from, to] as before.
    /// Bill-source entries:
    ///   - one-time (no Recurrence): same window check on `StartsAt` (= the bill's due date).
    ///   - recurring: the row stores the rule, not the occurrences. We pull every rule whose
    ///     life-of-the-rule overlaps the window, then expand each into one DTO per occurrence
    ///     inside the window. Open-ended bills (RecurrenceEndDate == null) are clamped at the
    ///     window's `to`, so a forever-monthly bill yields N entries for the requested month
    ///     instead of infinity.
    /// </summary>
    public async Task<IReadOnlyList<CalendarEventDto>> ListByHouseholdAsync(
        Guid householdId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var hid = HouseholdId.Create(householdId);
        // The ASP.NET query-binder hands us `DateTime` with Kind=Unspecified — Npgsql
        // refuses that against a `timestamptz` column. Force UTC at the seam.
        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);

        // Single query: pull every active row for the household whose window
        // *could* contribute an occurrence. We do the per-row windowing in C# so
        // the recurrence walk is shared between in-window one-times and rules.
        var rows = await db.CalendarEvents
            .AsNoTracking()
            .Where(e => e.HouseholdId == hid && e.DeletedAt == null
                        && (e.RecurrenceFrequency == null
                                ? e.StartsAt >= from && e.StartsAt <= to
                                : e.StartsAt <= to
                                  && (e.RecurrenceEndDate == null || e.RecurrenceEndDate >= from)))
            .ToListAsync(ct);

        var result = new List<CalendarEventDto>(rows.Count);
        foreach (var e in rows)
        {
            if (e.RecurrenceFrequency is null)
            {
                result.Add(MapInstance(e, e.StartsAt));
                continue;
            }
            foreach (var occ in RecurrenceExpander.EnumerateOccurrences(
                         e.StartsAt, e.RecurrenceFrequency.Value, e.RecurrenceEndDate, from, to))
                result.Add(MapInstance(e, occ));
        }

        return result
            .OrderBy(d => d.StartsAt)
            .ThenBy(d => d.Title)
            .ToList();
    }

    public async Task<CalendarEventDto?> GetByIdAsync(Guid eventId, CancellationToken ct = default)
    {
        var eid = CalendarEventId.Create(eventId);
        var e = await db.CalendarEvents.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eid && x.DeletedAt == null, ct);
        return e is null ? null : MapInstance(e, e.StartsAt);
    }

    private static CalendarEventDto MapInstance(HouseholdCalendarEvent e, DateTime startsAt) =>
        new(e.Id.Value, e.HouseholdId.Value, e.Title, e.Description,
            startsAt, e.EndsAt, e.AllDay, e.CreatedByUserId.Value, e.CreatedAt, e.UpdatedAt,
            e.Source == CalendarEventSource.FinanceBill ? CalendarEventKind.FinanceBill : CalendarEventKind.Member,
            e.LinkedExpenseId,
            e.RecurrenceFrequency,
            e.RecurrenceEndDate);
}
