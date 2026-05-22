namespace Household.Domain.Events;

public sealed record ChoreCreated(
    Guid ChoreId,
    Guid HouseholdId,
    Guid CreatedByUserId,
    string Title,
    string? Description,
    DateTime? DueDate,
    string? RecurrenceFrequency,
    DateTime CreatedAt) : DomainEvent;

public sealed record ChoreAssigned(
    Guid ChoreId,
    Guid HouseholdId,
    Guid AssignedToUserId,
    DateTime AssignedAt) : DomainEvent;

public sealed record ChoreCompleted(
    Guid ChoreId,
    Guid HouseholdId,
    Guid CompletedByUserId,
    DateTime CompletedAt) : DomainEvent;

public sealed record ChoreDeleted(
    Guid ChoreId,
    Guid HouseholdId,
    DateTime DeletedAt) : DomainEvent;
