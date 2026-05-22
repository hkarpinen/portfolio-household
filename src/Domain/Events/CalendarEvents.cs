namespace Household.Domain.Events;

public sealed record CalendarEventCreated(
    Guid CalendarEventId,
    Guid HouseholdId,
    Guid CreatedByUserId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    DateTime CreatedAt) : DomainEvent;

public sealed record CalendarEventUpdated(
    Guid CalendarEventId,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    DateTime UpdatedAt) : DomainEvent;

public sealed record CalendarEventDeleted(
    Guid CalendarEventId,
    Guid HouseholdId,
    DateTime DeletedAt) : DomainEvent;
