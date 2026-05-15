namespace Household.Application.Queries;

public interface ICalendarEventQuery
{
    Task<IReadOnlyList<CalendarEventDto>> ListByHouseholdAsync(
        Guid householdId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<CalendarEventDto?> GetByIdAsync(Guid eventId, CancellationToken ct = default);
}
