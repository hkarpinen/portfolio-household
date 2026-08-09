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

    // The two sources are deliberately asymmetric.
    //
    // Member-path mutators RAISE domain events: this is the source of truth for them.
    //
    // Bill-path mutators are PROJECTION-ONLY and raise none. The facts are already
    // owned elsewhere, so re-publishing them would emit a change this service never
    // authored — a feedback loop with no legitimate consumer.
    //
    // Recurrence is stored as a rule and expanded per query window; occurrences are
    // never materialised.
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
    /// Idempotent on `LinkedExpenseId`, which is the upsert key. `CreatedByUserId`
    /// is carried across so deletion-on-leave treats bill entries like member ones.
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

    /// <summary>Clears `DeletedAt`, so a reactivated bill returns idempotently.</summary>
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

    /// <summary>Soft-hide: the row is kept so reactivation is a single update.</summary>
    public void DeactivateFromBill()
    {
        if (Source != CalendarEventSource.FinanceBill)
            throw new InvalidOperationException("Only bill-sourced entries can be deactivated this way.");

        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

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
