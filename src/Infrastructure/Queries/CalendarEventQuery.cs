using Household.Application.Queries;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

internal sealed class CalendarEventQuery(HouseholdDbContext db) : ICalendarEventQuery
{
    // Member-source entries are windowed on StartsAt. Bill-source entries store the RULE, not the
    // occurrences: a one-time bill is windowed on StartsAt (its due date), a recurring one is pulled
    // if the life of its rule overlaps the window and then expanded into one DTO per occurrence
    // inside it. Open-ended bills (no end date) are clamped at the window's end, so a
    // forever-monthly bill yields N entries for the requested month instead of infinity.
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
