using Household.Domain.ValueObjects;

namespace Household.Application.Queries;

public sealed record ChoreDto(
    Guid Id,
    Guid HouseholdId,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    RecurrenceFrequency? RecurrenceFrequency,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    bool IsActive);
