using Household.Domain.ValueObjects;

namespace Household.Domain.Events;

public sealed record CalendarEventCreated(
    CalendarEventId CalendarEventId,
    HouseholdId HouseholdId,
    UserId CreatedByUserId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    DateTime CreatedAt) : DomainEvent;

public sealed record CalendarEventUpdated(
    CalendarEventId CalendarEventId,
    HouseholdId HouseholdId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    DateTime UpdatedAt) : DomainEvent;

public sealed record CalendarEventDeleted(
    CalendarEventId CalendarEventId,
    HouseholdId HouseholdId,
    DateTime DeletedAt) : DomainEvent;
