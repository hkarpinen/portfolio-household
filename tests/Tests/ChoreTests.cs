using Household.Domain.Aggregates;
using Household.Domain.Events;
using Household.Domain.ValueObjects;

namespace Tests;

public class ChoreTests
{
    private static UserId NewUserId() => UserId.Create(Guid.NewGuid());
    private static HouseholdId NewHouseholdId() => HouseholdId.New();

    private static Chore CreateChore() =>
        Chore.Create(NewHouseholdId(), NewUserId(), "Wash dishes", null, null, null);

    [Fact]
    public void Create_SetsProperties()
    {
        var householdId = NewHouseholdId();
        var userId = NewUserId();
        var dueDate = DateTime.UtcNow.AddDays(1);

        var chore = Chore.Create(householdId, userId, "Clean floors", "With mop", dueDate, RecurrenceFrequency.Weekly);

        Assert.Equal("Clean floors", chore.Title);
        Assert.Equal("With mop", chore.Description);
        Assert.Equal(householdId, chore.HouseholdId);
        Assert.Equal(userId, chore.CreatedByUserId);
        Assert.Equal(dueDate, chore.DueDate);
        Assert.Equal(RecurrenceFrequency.Weekly, chore.RecurrenceFrequency);
        Assert.True(chore.IsActive);
        Assert.Null(chore.CompletedAt);
        Assert.Null(chore.AssignedToUserId);
    }

    [Fact]
    public void Create_RaisesChoreCreatedEvent()
    {
        var chore = CreateChore();

        Assert.Single(chore.DomainEvents);
        Assert.IsType<ChoreCreated>(chore.DomainEvents.First());
    }

    [Fact]
    public void Assign_SetsAssignedUserId()
    {
        var chore = CreateChore();
        var assignee = NewUserId();
        chore.ClearDomainEvents();

        chore.Assign(assignee);

        Assert.Equal(assignee, chore.AssignedToUserId);
    }

    [Fact]
    public void Assign_RaisesChoreAssignedEvent()
    {
        var chore = CreateChore();
        chore.ClearDomainEvents();

        chore.Assign(NewUserId());

        Assert.Single(chore.DomainEvents);
        Assert.IsType<ChoreAssigned>(chore.DomainEvents.First());
    }

    [Fact]
    public void Complete_SetsCompletedAtAndDeactivates()
    {
        var chore = CreateChore();
        chore.ClearDomainEvents();

        chore.Complete(NewUserId());

        Assert.NotNull(chore.CompletedAt);
        Assert.False(chore.IsActive);
    }

    [Fact]
    public void Complete_RaisesChoreCompletedEvent()
    {
        var chore = CreateChore();
        chore.ClearDomainEvents();

        chore.Complete(NewUserId());

        Assert.Single(chore.DomainEvents);
        Assert.IsType<ChoreCompleted>(chore.DomainEvents.First());
    }

    [Fact]
    public void Delete_SetsIsActiveFalse()
    {
        var chore = CreateChore();
        chore.ClearDomainEvents();

        chore.Delete();

        Assert.False(chore.IsActive);
    }

    [Fact]
    public void Delete_RaisesChoreDeletedEvent()
    {
        var chore = CreateChore();
        chore.ClearDomainEvents();

        chore.Delete();

        Assert.Single(chore.DomainEvents);
        Assert.IsType<ChoreDeleted>(chore.DomainEvents.First());
    }
}
