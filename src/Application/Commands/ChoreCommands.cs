using Household.Domain.ValueObjects;

namespace Household.Application.Commands;

public sealed record CreateChoreCommand(
    Guid HouseholdId,
    Guid RequestingUserId,
    string Title,
    string? Description,
    DateTime? DueDate,
    RecurrenceFrequency? RecurrenceFrequency);

public sealed record AssignChoreCommand(
    Guid ChoreId,
    Guid HouseholdId,
    Guid RequestingUserId,
    Guid AssignToUserId);

public sealed record CompleteChoreCommand(
    Guid ChoreId,
    Guid HouseholdId,
    Guid RequestingUserId);

public sealed record DeleteChoreCommand(
    Guid ChoreId,
    Guid HouseholdId,
    Guid RequestingUserId);
