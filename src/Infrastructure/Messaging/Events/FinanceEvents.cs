// Wire contracts for finance events consumed from RabbitMQ.
//
// The namespace and type names MUST match the domain-event records finance
// publishes (see finance/src/Domain/Events/ExpenseEvents.cs and
// ExpenseSplitEvents.cs). MassTransit routes messages by namespace+type, so
// finance's `Finance.Domain.Events.ChargeCreated` and household's
// `Finance.Domain.Events.ChargeCreated` below resolve to the same exchange.
//
// Finance flattens its value-object IDs (ExpenseId/ExpenseSplitId/GroupId/UserId)
// to bare GUIDs via its OutboxExtensions converters, so consumers see plain Guid
// properties. Finance's DomainEvent base contributes the EventId/OccurredAt
// fields. Extra properties on the wire are deserialised away — only the fields
// household actually projects are modelled here.
namespace Finance.Domain.Events;

/// <summary>
/// Finance recurrence schedule on the wire, per finance's RecurrenceScheduleConverter.
/// `Frequency` is a camelCase enum-name string ("daily" | "weekly" | "biWeekly" |
/// "monthly" | "quarterly" | "semiAnnually" | "annually"); the consumer parses it
/// into household's own `RecurrenceFrequency` enum (same names, case-insensitive).
/// </summary>
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

// Finance models a member settling their share as a Settlement — one journal entry in the
// group ledger. Only the fields the activity feed reads are modelled here; extra wire fields
// (toUserId, amount, valueDate, settlementEntryId) are deserialised away. Field names must
// match finance's wire (allocationId, chargeId).
public sealed record SettlementRecorded(
    Guid EventId,
    DateTime OccurredAt,
    Guid AllocationId,
    Guid ChargeId,
    Guid GroupId,
    Guid FromUserId,
    DateTime OccurrenceDate);
