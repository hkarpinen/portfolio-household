namespace Household.Domain.ValueObjects;

public static class RecurrenceExpander
{
    // Stops at endDate if set, otherwise clamps at `to`. The loop is bounded by the window and never
    // infinite — an open-ended monthly rule yields N occurrences for the window, not an endless walk.
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
