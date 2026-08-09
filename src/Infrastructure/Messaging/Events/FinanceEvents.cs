// Messages route by namespace and type name, so these MUST match the publisher's
// exactly — a mismatch binds a different exchange and every message is missed
// silently. Ids arrive as bare GUIDs; unmodelled wire fields deserialise away.
namespace Finance.Domain.Events;

/// <summary>`Frequency` arrives as a camelCase enum NAME, not a number.</summary>
public sealed record FinanceRecurrenceSchedule(
    string Frequency,
    DateTime StartDate,
    DateTime? EndDate);

public sealed record ChargeCreated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ChargeId,
    Guid UserId,
    string Title,
    DateTime DueDate,
    FinanceRecurrenceSchedule? RecurrenceSchedule,
    Guid? GroupId);

public sealed record ChargeUpdated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ChargeId,
    string Title,
    DateTime DueDate,
    FinanceRecurrenceSchedule? RecurrenceSchedule,
    Guid? GroupId);

public sealed record ChargeDeactivated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ChargeId,
    Guid? GroupId);

public sealed record ChargeActivated(
    Guid EventId,
    DateTime OccurredAt,
    Guid ChargeId,
    Guid? GroupId);

// Only the fields the feed reads are modelled; the names must match the wire exactly.
public sealed record SettlementRecorded(
    Guid EventId,
    DateTime OccurredAt,
    Guid AllocationId,
    Guid ChargeId,
    Guid GroupId,
    Guid FromUserId,
    DateTime OccurrenceDate);
