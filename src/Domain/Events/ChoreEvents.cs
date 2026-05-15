using Household.Domain.ValueObjects;

namespace Household.Domain.Events;

public sealed record ChoreCreated(
    ChoreId ChoreId,
    HouseholdId HouseholdId,
    UserId CreatedByUserId,
    string Title,
    string? Description,
    DateTime? DueDate,
    RecurrenceFrequency? RecurrenceFrequency,
    DateTime CreatedAt) : DomainEvent;

public sealed record ChoreAssigned(
    ChoreId ChoreId,
    HouseholdId HouseholdId,
    UserId AssignedToUserId,
    DateTime AssignedAt) : DomainEvent;

public sealed record ChoreCompleted(
    ChoreId ChoreId,
    HouseholdId HouseholdId,
    UserId CompletedByUserId,
    DateTime CompletedAt) : DomainEvent;

public sealed record ChoreDeleted(
    ChoreId ChoreId,
    HouseholdId HouseholdId,
    DateTime DeletedAt) : DomainEvent;
