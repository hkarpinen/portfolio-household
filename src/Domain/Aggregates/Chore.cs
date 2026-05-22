using Household.Domain.Events;
using Household.Domain.ValueObjects;

namespace Household.Domain.Aggregates;

public sealed class Chore : IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    public ChoreId Id { get; private set; }
    public HouseholdId HouseholdId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public UserId? AssignedToUserId { get; private set; }
    public DateTime? DueDate { get; private set; }
    public RecurrenceFrequency? RecurrenceFrequency { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Chore() { }

    public static Chore Create(
        HouseholdId householdId,
        UserId createdByUserId,
        string title,
        string? description,
        DateTime? dueDate,
        RecurrenceFrequency? recurrenceFrequency)
    {
        var now = DateTime.UtcNow;
        var chore = new Chore
        {
            Id = ChoreId.New(),
            HouseholdId = householdId,
            CreatedByUserId = createdByUserId,
            Title = title,
            Description = description,
            DueDate = dueDate,
            RecurrenceFrequency = recurrenceFrequency,
            CreatedAt = now,
            IsActive = true
        };
        chore._domainEvents.Add(new ChoreCreated(
            chore.Id.Value, householdId.Value, createdByUserId.Value, title, description, dueDate,
            recurrenceFrequency?.ToString(), now));
        return chore;
    }

    public void Assign(UserId assignedToUserId)
    {
        AssignedToUserId = assignedToUserId;
        _domainEvents.Add(new ChoreAssigned(Id.Value, HouseholdId.Value, assignedToUserId.Value, DateTime.UtcNow));
    }

    public void Complete(UserId completedByUserId)
    {
        CompletedAt = DateTime.UtcNow;
        IsActive = false;
        _domainEvents.Add(new ChoreCompleted(Id.Value, HouseholdId.Value, completedByUserId.Value, CompletedAt.Value));
    }

    public void Delete()
    {
        IsActive = false;
        _domainEvents.Add(new ChoreDeleted(Id.Value, HouseholdId.Value, DateTime.UtcNow));
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
