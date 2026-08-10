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
    public UserId? CompletedByUserId { get; private set; }
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

    // A completed chore is history: Complete() has already spawned its successor as a separate
    // row, so editing the finished one changes nothing going forward and leaves the two
    // disagreeing about what the chore is. Edit the successor instead.
    public void Update(
        string title,
        string? description,
        DateTime? dueDate,
        RecurrenceFrequency? recurrenceFrequency)
    {
        if (CompletedAt is not null)
            throw new InvalidOperationException("A chore that is already done cannot be changed.");

        Title = title;
        Description = description;
        DueDate = dueDate;
        RecurrenceFrequency = recurrenceFrequency;
        _domainEvents.Add(new ChoreUpdated(
            Id.Value, HouseholdId.Value, title, description, dueDate,
            recurrenceFrequency?.ToString(), DateTime.UtcNow));
    }

    public void Assign(UserId assignedToUserId)
    {
        AssignedToUserId = assignedToUserId;
        _domainEvents.Add(new ChoreAssigned(Id.Value, HouseholdId.Value, assignedToUserId.Value, DateTime.UtcNow));
    }

    public void Complete(UserId completedByUserId)
    {
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = completedByUserId;
        IsActive = false;
        _domainEvents.Add(new ChoreCompleted(Id.Value, HouseholdId.Value, completedByUserId.Value, CompletedAt.Value));
    }

    // Stepped from THIS occurrence's due date, not from the completion time, so a chore stays on its
    // cadence — a Wednesday bin day done late on Friday is still due the next Wednesday. Stepped
    // forward until it lands after the completion, so one finished several cycles late does not come
    // back already overdue. The assignee carries over.
    public Chore? CreateNextOccurrence(DateTime completedAt)
    {
        if (RecurrenceFrequency is null || DueDate is null) return null;

        var next = RecurrenceExpander.Advance(DueDate.Value, RecurrenceFrequency.Value);
        while (next <= completedAt)
            next = RecurrenceExpander.Advance(next, RecurrenceFrequency.Value);

        var chore = Create(HouseholdId, CreatedByUserId, Title, Description, next, RecurrenceFrequency);
        if (AssignedToUserId is { } assignee)
            chore.Assign(assignee);
        return chore;
    }

    public void Delete()
    {
        IsActive = false;
        _domainEvents.Add(new ChoreDeleted(Id.Value, HouseholdId.Value, DateTime.UtcNow));
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
