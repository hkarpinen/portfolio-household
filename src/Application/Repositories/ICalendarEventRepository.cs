using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;

namespace Household.Application.Repositories;

public interface ICalendarEventRepository
{
    Task<HouseholdCalendarEvent?> GetByIdAsync(CalendarEventId id, CancellationToken ct = default);
    Task AddAsync(HouseholdCalendarEvent calendarEvent, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task DeleteByHouseholdIdsAsync(IEnumerable<HouseholdId> householdIds, CancellationToken ct = default);
}
