using Household.Domain.Events;
using Household.Domain.ValueObjects;

namespace Household.Domain.Aggregates;

public sealed class HouseholdCalendarEvent : IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    public CalendarEventId Id { get; private set; }
    public HouseholdId HouseholdId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool AllDay { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // ── Source-aware fields ──────────────────────────────────────────────────
    // `Source = Member` is the existing behaviour. `Source = FinanceBill` events
    // are mirrored from finance's expense lifecycle; they carry a `LinkedExpenseId`
    // and (for recurring bills) a frequency + optional end date. Recurrence is
    // expanded at query time across the requested calendar window — we don't
    // materialise N occurrences per recurring bill.
    //
    // ── Event contract (intentionally asymmetric) ────────────────────────────
    // Member-path mutators (Create/Update/Delete) RAISE domain events
    // (CalendarEventCreated/Updated/Deleted): household is the source of truth for
    // member events, so those drain to the outbox and feed the activity projector.
    //
    // Bill-path mutators (CreateFromBill/UpdateFromBill/Deactivate/ActivateFromBill)
    // are PROJECTION-ONLY: they are written by the finance-event consumer
    // (Infrastructure.Messaging.Consumers.FinanceConsumers) as a local read-model
    // mirror of finance's authoritative Charge lifecycle. They deliberately raise NO
    // domain events — finance already owns those facts, and echoing them back to the
    // outbox would re-publish a change household didn't author (a feedback loop with
    // no legitimate consumer). The activity feed for bill changes is written directly
    // by the consumer, not via these mutators. `BillMutatorsAreProjectionOnly` in
    // HouseholdCalendarEventTests guards this invariant so the asymmetry can't drift.
    public CalendarEventSource Source { get; private set; }
    public Guid? LinkedExpenseId { get; private set; }
    public RecurrenceFrequency? RecurrenceFrequency { get; private set; }
    public DateTime? RecurrenceEndDate { get; private set; }

    private HouseholdCalendarEvent() { }

    public static HouseholdCalendarEvent Create(
        HouseholdId householdId,
        UserId createdByUserId,
        string title,
        string? description,
        DateTime startsAt,
        DateTime? endsAt,
        bool allDay,
        RecurrenceFrequency? recurrenceFrequency = null,
        DateTime? recurrenceEndDate = null)
    {
        var now = DateTime.UtcNow;
        var ev = new HouseholdCalendarEvent
        {
            Id = CalendarEventId.New(),
            HouseholdId = householdId,
            CreatedByUserId = createdByUserId,
            Source = CalendarEventSource.Member,
            Title = title,
            Description = description,
            StartsAt = startsAt,
            EndsAt = endsAt,
            AllDay = allDay,
            RecurrenceFrequency = recurrenceFrequency,
            RecurrenceEndDate = recurrenceEndDate,
            CreatedAt = now
        };
        ev._domainEvents.Add(new CalendarEventCreated(
            ev.Id.Value, householdId.Value, createdByUserId.Value, title, description, startsAt, endsAt, allDay, now));
        return ev;
    }

    /// <summary>
    /// Mirror a finance shared-expense onto the calendar. Idempotent at the
    /// query layer via `LinkedExpenseId`; the consumer upserts by that key.
    /// `CreatedByUserId` is the finance event's `CreatedBy` so deletion-on-leave
    /// semantics treat bill entries the same way member events are treated.
    /// </summary>
    public static HouseholdCalendarEvent CreateFromBill(
        HouseholdId householdId,
        UserId createdByUserId,
        Guid linkedExpenseId,
        string title,
        DateTime dueDate,
        RecurrenceFrequency? recurrenceFrequency,
        DateTime? recurrenceEndDate)
    {
        var now = DateTime.UtcNow;
        return new HouseholdCalendarEvent
        {
            Id = CalendarEventId.New(),
            HouseholdId = householdId,
            CreatedByUserId = createdByUserId,
            Source = CalendarEventSource.FinanceBill,
            LinkedExpenseId = linkedExpenseId,
            Title = title,
            StartsAt = dueDate,
            AllDay = true,
            RecurrenceFrequency = recurrenceFrequency,
            RecurrenceEndDate = recurrenceEndDate,
            CreatedAt = now
        };
    }

    public void Update(
        string title,
        string? description,
        DateTime startsAt,
        DateTime? endsAt,
        bool allDay,
        RecurrenceFrequency? recurrenceFrequency = null,
        DateTime? recurrenceEndDate = null)
    {
        if (Source != CalendarEventSource.Member)
            throw new InvalidOperationException("Bill-sourced calendar entries are read-only — edit the upstream bill.");

        Title = title;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
        AllDay = allDay;
        RecurrenceFrequency = recurrenceFrequency;
        RecurrenceEndDate = recurrenceEndDate;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new CalendarEventUpdated(Id.Value, HouseholdId.Value, title, description, startsAt, endsAt, allDay, UpdatedAt.Value));
    }

    /// <summary>
    /// Apply a finance ExpenseUpdated to a bill-sourced entry. Also clears
    /// `DeletedAt` so a re-activated bill (deactivated → reactivated) returns
    /// to the calendar idempotently. Projection-only: raises no domain event
    /// (finance owns this fact — see the event-contract note at the top of the file).
    /// </summary>
    public void UpdateFromBill(string title, DateTime dueDate, RecurrenceFrequency? recurrenceFrequency, DateTime? recurrenceEndDate)
    {
        if (Source != CalendarEventSource.FinanceBill)
            throw new InvalidOperationException("Cannot apply a bill update to a member-created event.");

        Title = title;
        StartsAt = dueDate;
        RecurrenceFrequency = recurrenceFrequency;
        RecurrenceEndDate = recurrenceEndDate;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (Source != CalendarEventSource.Member)
            throw new InvalidOperationException("Bill-sourced calendar entries are removed via the upstream bill.");

        DeletedAt = DateTime.UtcNow;
        _domainEvents.Add(new CalendarEventDeleted(Id.Value, HouseholdId.Value, DeletedAt.Value));
    }

    /// <summary>Soft-hide a bill entry on `ExpenseDeactivated`; row is kept so re-activation is a single update. Projection-only — raises no domain event.</summary>
    public void DeactivateFromBill()
    {
        if (Source != CalendarEventSource.FinanceBill)
            throw new InvalidOperationException("Only bill-sourced entries can be deactivated this way.");

        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Restore a previously-deactivated bill entry on `ExpenseActivated`. Projection-only — raises no domain event.</summary>
    public void ActivateFromBill()
    {
        if (Source != CalendarEventSource.FinanceBill)
            throw new InvalidOperationException("Only bill-sourced entries can be reactivated this way.");

        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
