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


    [Fact]
    public void Update_ChangesTheFieldsAndRaisesTheEvent()
    {
        var chore = CreateChore();
        chore.ClearDomainEvents();
        var due = DateTime.UtcNow.AddDays(3);

        chore.Update("Wash the dishes properly", "Including the pans", due, RecurrenceFrequency.Weekly);

        Assert.Equal("Wash the dishes properly", chore.Title);
        Assert.Equal("Including the pans", chore.Description);
        Assert.Equal(due, chore.DueDate);
        Assert.Equal(RecurrenceFrequency.Weekly, chore.RecurrenceFrequency);
        Assert.Single(chore.DomainEvents.OfType<ChoreUpdated>());
    }

    [Fact]
    public void Update_ClearsRecurrence_WhenNoneIsGiven()
    {
        var chore = Chore.Create(NewHouseholdId(), NewUserId(), "Bins", null, DateTime.UtcNow, RecurrenceFrequency.Weekly);

        chore.Update("Bins", null, DateTime.UtcNow, null);

        Assert.Null(chore.RecurrenceFrequency);
        Assert.Null(chore.CreateNextOccurrence(DateTime.UtcNow));
    }

    [Fact]
    public void Update_Throws_WhenTheChoreIsAlreadyDone()
    {
        var chore = CreateChore();
        chore.Complete(NewUserId());

        // The successor is its own row by then; editing this one would leave the two
        // disagreeing about what the chore is.
        Assert.Throws<InvalidOperationException>(
            () => chore.Update("Something else", null, null, null));
    }

    [Fact]
    public void CreateNextOccurrence_ReturnsNull_WhenChoreDoesNotRepeat()
    {
        var chore = Chore.Create(NewHouseholdId(), NewUserId(), "Wash dishes", null, DateTime.UtcNow, null);

        Assert.Null(chore.CreateNextOccurrence(DateTime.UtcNow));
    }

    [Fact]
    public void CreateNextOccurrence_ReturnsNull_WhenChoreHasNoDueDate()
    {
        var chore = Chore.Create(NewHouseholdId(), NewUserId(), "Wash dishes", null, null, RecurrenceFrequency.Weekly);

        Assert.Null(chore.CreateNextOccurrence(DateTime.UtcNow));
    }

    [Fact]
    public void CreateNextOccurrence_StepsOneCycleFromTheDueDate()
    {
        var due = new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc);
        var chore = Chore.Create(NewHouseholdId(), NewUserId(), "Take the bins out", "Kerb by 7am", due, RecurrenceFrequency.Weekly);

        var next = chore.CreateNextOccurrence(due);

        Assert.NotNull(next);
        Assert.Equal(due.AddDays(7), next!.DueDate);
        Assert.Equal("Take the bins out", next.Title);
        Assert.Equal("Kerb by 7am", next.Description);
        Assert.Equal(RecurrenceFrequency.Weekly, next.RecurrenceFrequency);
        Assert.True(next.IsActive);
        Assert.Null(next.CompletedAt);
    }

    [Fact]
    public void CreateNextOccurrence_StaysOnCadence_WhenCompletedLate()
    {
        // Due Wednesday, actually done 16 days later. The next one is the
        // following Wednesday after that, not "16 days from now" and not a
        // date already in the past.
        var due = new DateTime(2026, 8, 5, 9, 0, 0, DateTimeKind.Utc);
        var chore = Chore.Create(NewHouseholdId(), NewUserId(), "Take the bins out", null, due, RecurrenceFrequency.Weekly);

        var next = chore.CreateNextOccurrence(due.AddDays(16));

        Assert.NotNull(next);
        Assert.Equal(due.AddDays(21), next!.DueDate);
        Assert.True(next.DueDate > due.AddDays(16));
    }

    [Fact]
    public void CreateNextOccurrence_CarriesTheAssigneeOver()
    {
        var due = DateTime.UtcNow;
        var assignee = NewUserId();
        var chore = Chore.Create(NewHouseholdId(), NewUserId(), "Water the plants", null, due, RecurrenceFrequency.Monthly);
        chore.Assign(assignee);

        var next = chore.CreateNextOccurrence(due);

        Assert.Equal(assignee, next!.AssignedToUserId);
    }

    [Fact]
    public void CreateNextOccurrence_DoesNotMutateTheCompletedChore()
    {
        var due = DateTime.UtcNow;
        var chore = Chore.Create(NewHouseholdId(), NewUserId(), "Hoover", null, due, RecurrenceFrequency.Daily);
        chore.Complete(NewUserId());

        var next = chore.CreateNextOccurrence(due);

        Assert.False(chore.IsActive);
        Assert.NotNull(chore.CompletedAt);
        Assert.NotEqual(chore.Id, next!.Id);
    }
}
