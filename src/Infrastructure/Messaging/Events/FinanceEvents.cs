// Messages route by namespace and type name, so these MUST match the publisher's
// exactly — a mismatch binds a different exchange and every message is missed
// silently. Ids arrive as bare GUIDs; unmodelled wire fields deserialise away.
namespace Finance.Domain.Events;

/// <summary>`Frequency` arrives as a camelCase enum NAME, not a number.</summary>
public sealed record FinanceRecurrenceSchedule(
    string Frequency,
    DateTime StartDate,
    DateTime? EndDate);

public sealed record ExpenseCreated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ExpenseId,
    Guid UserId,
    string Title,
    DateTime DueDate,
    FinanceRecurrenceSchedule? RecurrenceSchedule,
    Guid? GroupId);

public sealed record ExpenseUpdated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ExpenseId,
    string Title,
    DateTime DueDate,
    FinanceRecurrenceSchedule? RecurrenceSchedule,
    Guid? GroupId);

public sealed record ExpenseDeactivated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ExpenseId,
    Guid? GroupId);

public sealed record ExpenseActivated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ExpenseId,
    Guid? GroupId);

// Only the fields the feed reads are modelled; the names must match the wire exactly.
public sealed record SettlementRecorded(
    Guid EventId,
    DateTime OccurredAt,
    Guid ShareId,
    Guid ExpenseId,
    Guid GroupId,
    Guid FromUserId,
    DateTime OccurrenceDate);
