namespace Household.Application.Queries;

public sealed record CalendarEventDto(
    Guid Id,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
