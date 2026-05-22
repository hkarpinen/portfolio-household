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

    private HouseholdCalendarEvent() { }

    public static HouseholdCalendarEvent Create(
        HouseholdId householdId,
        UserId createdByUserId,
        string title,
        string? description,
        DateTime startsAt,
        DateTime? endsAt,
        bool allDay)
    {
        var now = DateTime.UtcNow;
        var ev = new HouseholdCalendarEvent
        {
            Id = CalendarEventId.New(),
            HouseholdId = householdId,
            CreatedByUserId = createdByUserId,
            Title = title,
            Description = description,
            StartsAt = startsAt,
            EndsAt = endsAt,
            AllDay = allDay,
            CreatedAt = now
        };
        ev._domainEvents.Add(new CalendarEventCreated(
            ev.Id.Value, householdId.Value, createdByUserId.Value, title, description, startsAt, endsAt, allDay, now));
        return ev;
    }

    public void Update(string title, string? description, DateTime startsAt, DateTime? endsAt, bool allDay)
    {
        Title = title;
        Description = description;
        StartsAt = startsAt;
        EndsAt = endsAt;
        AllDay = allDay;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new CalendarEventUpdated(Id.Value, HouseholdId.Value, title, description, startsAt, endsAt, allDay, UpdatedAt.Value));
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        _domainEvents.Add(new CalendarEventDeleted(Id.Value, HouseholdId.Value, DeletedAt.Value));
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
