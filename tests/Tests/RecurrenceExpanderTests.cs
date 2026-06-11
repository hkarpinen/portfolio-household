using Household.Domain.ValueObjects;

namespace Tests;

public class RecurrenceExpanderTests
{
    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Monthly_OpenEnded_ClampsAtWindowTo()
    {
        var start = Utc(2026, 1, 15);
        var from = Utc(2026, 1, 1);
        var to = Utc(2026, 6, 30);

        var occ = RecurrenceExpander
            .EnumerateOccurrences(start, RecurrenceFrequency.Monthly, endDate: null, from, to)
            .ToList();

        // Jan..Jun on the 15th — six occurrences, never infinite despite the null end date.
        Assert.Equal(6, occ.Count);
        Assert.Equal(Utc(2026, 1, 15), occ[0]);
        Assert.Equal(Utc(2026, 6, 15), occ[^1]);
    }

    [Fact]
    public void Monthly_RespectsEndDateBeforeWindowEnd()
    {
        var occ = RecurrenceExpander
            .EnumerateOccurrences(Utc(2026, 1, 15), RecurrenceFrequency.Monthly,
                endDate: Utc(2026, 3, 20), from: Utc(2026, 1, 1), to: Utc(2026, 12, 31))
            .ToList();

        Assert.Equal(3, occ.Count); // Jan, Feb, Mar
        Assert.Equal(Utc(2026, 3, 15), occ[^1]);
    }

    [Fact]
    public void Weekly_SkipsOccurrencesBeforeFrom()
    {
        var occ = RecurrenceExpander
            .EnumerateOccurrences(Utc(2026, 1, 1), RecurrenceFrequency.Weekly,
                endDate: null, from: Utc(2026, 1, 15), to: Utc(2026, 1, 31))
            .ToList();

        Assert.All(occ, d => Assert.True(d >= Utc(2026, 1, 15)));
        Assert.Equal(Utc(2026, 1, 15), occ[0]); // Jan 1 + 14d
        Assert.Equal(Utc(2026, 1, 29), occ[^1]);
    }

    [Fact]
    public void StartAfterWindow_YieldsNothing()
    {
        var occ = RecurrenceExpander
            .EnumerateOccurrences(Utc(2026, 5, 1), RecurrenceFrequency.Daily,
                endDate: null, from: Utc(2026, 1, 1), to: Utc(2026, 1, 31))
            .ToList();

        Assert.Empty(occ);
    }

    [Theory]
    [InlineData(RecurrenceFrequency.Daily)]
    [InlineData(RecurrenceFrequency.Weekly)]
    [InlineData(RecurrenceFrequency.BiWeekly)]
    [InlineData(RecurrenceFrequency.Monthly)]
    [InlineData(RecurrenceFrequency.Quarterly)]
    [InlineData(RecurrenceFrequency.SemiAnnually)]
    [InlineData(RecurrenceFrequency.Annually)]
    public void Advance_MovesForward(RecurrenceFrequency f)
    {
        var start = Utc(2026, 1, 1);
        Assert.True(RecurrenceExpander.Advance(start, f) > start);
    }
}
