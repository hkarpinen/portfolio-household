namespace Household.Domain.ValueObjects;

/// <summary>
/// Pure, side-effect-free expansion of a recurrence rule into concrete occurrences.
/// This is DOMAIN logic (what a recurring rule *means* over a window) and lives next
/// to <see cref="RecurrenceFrequency"/> so it can be unit-tested without a DbContext.
/// Query/Resource-Access classes call into this instead of re-implementing the walk.
/// </summary>
public static class RecurrenceExpander
{
    /// <summary>
    /// Step from <paramref name="start"/> by <paramref name="frequency"/>, yielding every
    /// occurrence that falls in <c>[from, to]</c>. Stops at <paramref name="endDate"/> if set;
    /// otherwise clamps at <paramref name="to"/>. The loop is bounded by the window, never
    /// infinite — an open-ended (null end date) monthly bill yields N entries for the
    /// requested window instead of running forever.
    /// </summary>
    public static IEnumerable<DateTime> EnumerateOccurrences(
        DateTime start, RecurrenceFrequency frequency, DateTime? endDate, DateTime from, DateTime to)
    {
        var hardStop = endDate.HasValue && endDate.Value < to ? endDate.Value : to;
        var cursor = start;
        while (cursor <= hardStop)
        {
            if (cursor >= from)
                yield return cursor;
            cursor = Advance(cursor, frequency);
        }
    }

    /// <summary>Advance a single cursor by one step of the given frequency.</summary>
    public static DateTime Advance(DateTime d, RecurrenceFrequency f) => f switch
    {
        RecurrenceFrequency.Daily        => d.AddDays(1),
        RecurrenceFrequency.Weekly       => d.AddDays(7),
        RecurrenceFrequency.BiWeekly     => d.AddDays(14),
        RecurrenceFrequency.Monthly      => d.AddMonths(1),
        RecurrenceFrequency.Quarterly    => d.AddMonths(3),
        RecurrenceFrequency.SemiAnnually => d.AddMonths(6),
        RecurrenceFrequency.Annually     => d.AddYears(1),
        _                                => throw new InvalidOperationException($"Unknown frequency: {f}")
    };
}
