using Household.Domain.ValueObjects;

namespace Household.Application.Dtos;

public sealed record CreateCalendarEventRequest(
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    RecurrenceFrequency? RecurrenceFrequency = null,
    DateTime? RecurrenceEndDate = null);

public sealed record UpdateCalendarEventRequest(
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    RecurrenceFrequency? RecurrenceFrequency = null,
    DateTime? RecurrenceEndDate = null);
