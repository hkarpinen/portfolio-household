namespace Household.Application.Commands;

public sealed record CreateCalendarEventCommand(
    Guid HouseholdId,
    Guid RequestingUserId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay);

public sealed record UpdateCalendarEventCommand(
    Guid CalendarEventId,
    Guid HouseholdId,
    Guid RequestingUserId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay);

public sealed record DeleteCalendarEventCommand(
    Guid CalendarEventId,
    Guid HouseholdId,
    Guid RequestingUserId);
