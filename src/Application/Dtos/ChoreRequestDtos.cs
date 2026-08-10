using Household.Domain.ValueObjects;

namespace Household.Application.Dtos;

public sealed record CreateChoreRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    RecurrenceFrequency? RecurrenceFrequency,
    Guid? InitialAssigneeUserId = null);
public sealed record UpdateChoreRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    RecurrenceFrequency? RecurrenceFrequency);
public sealed record AssignChoreRequest(Guid AssignToUserId);
