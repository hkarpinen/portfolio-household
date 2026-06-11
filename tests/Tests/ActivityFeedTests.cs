using Household.Application.Dtos;
using Household.Domain.Events;
using Infrastructure.Persistence;

namespace Tests;

public class ActivityFeedTests
{
    // -----------------------------------------------------------------------
    // ActivityEventRecord — basic POCO round-trip
    // -----------------------------------------------------------------------

    [Fact]
    public void ActivityEventRecord_Properties_RoundTrip()
    {
        var id = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var record = new ActivityEventRecord
        {
            Id = id,
            HouseholdId = householdId,
            EventType = nameof(ActivityEventType.ChoreCompleted),
            ActorId = actorId,
            ActorDisplayName = "Alice",
            TargetId = targetId,
            TargetDescription = "Clean the kitchen",
            OccurredAt = now,
        };

        Assert.Equal(id, record.Id);
        Assert.Equal(householdId, record.HouseholdId);
        Assert.Equal(nameof(ActivityEventType.ChoreCompleted), record.EventType);
        Assert.Equal(actorId, record.ActorId);
        Assert.Equal("Alice", record.ActorDisplayName);
        Assert.Equal(targetId, record.TargetId);
        Assert.Equal("Clean the kitchen", record.TargetDescription);
        Assert.Equal(now, record.OccurredAt);
    }

    // -----------------------------------------------------------------------
    // ActivityFeedProjector — unit tests, no DbContext, no EF
    // -----------------------------------------------------------------------

    [Fact]
    public void TryProject_HouseholdMemberJoined_MapsToMemberJoined()
    {
        var membershipId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var joinedAt = DateTime.UtcNow;

        var domainEvent = new HouseholdMemberJoined(
            MembershipId: membershipId,
            HouseholdId: householdId,
            UserId: userId,
            Role: "Member",
            JoinedAt: joinedAt);

        var record = ActivityFeedProjector.TryProject(domainEvent, _ => "Bob");

        Assert.NotNull(record);
        Assert.Equal(householdId, record!.HouseholdId);
        Assert.Equal(nameof(ActivityEventType.MemberJoined), record.EventType);
        Assert.Equal(userId, record.ActorId);
        Assert.Equal("Bob", record.ActorDisplayName);
        Assert.Equal(membershipId, record.TargetId);
        Assert.Equal("Member", record.TargetDescription);
        Assert.Equal(joinedAt, record.OccurredAt);
    }

    [Fact]
    public void TryProject_ChoreCompleted_MapsToChoreCompleted()
    {
        var choreId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var completedAt = DateTime.UtcNow;

        var domainEvent = new ChoreCompleted(
            ChoreId: choreId,
            HouseholdId: householdId,
            CompletedByUserId: userId,
            CompletedAt: completedAt);

        var record = ActivityFeedProjector.TryProject(domainEvent, _ => "Carol");

        Assert.NotNull(record);
        Assert.Equal(householdId, record!.HouseholdId);
        Assert.Equal(nameof(ActivityEventType.ChoreCompleted), record.EventType);
        Assert.Equal(userId, record.ActorId);
        Assert.Equal("Carol", record.ActorDisplayName);
        Assert.Equal(choreId, record.TargetId);
        Assert.Null(record.TargetDescription);
        Assert.Equal(completedAt, record.OccurredAt);
    }

    [Fact]
    public void TryProject_UnmappedEvent_ReturnsNull()
    {
        var domainEvent = new ChoreDeleted(
            ChoreId: Guid.NewGuid(),
            HouseholdId: Guid.NewGuid(),
            DeletedAt: DateTime.UtcNow);

        var record = ActivityFeedProjector.TryProject(domainEvent, _ => "Dave");

        Assert.Null(record);
    }

    [Fact]
    public void TryProject_DisplayNameFallsBackToEmptyString_WhenResolverReturnsNull()
    {
        var domainEvent = new CalendarEventCreated(
            CalendarEventId: Guid.NewGuid(),
            HouseholdId: Guid.NewGuid(),
            CreatedByUserId: Guid.NewGuid(),
            Title: "Team meeting",
            Description: null,
            StartsAt: DateTime.UtcNow,
            EndsAt: null,
            AllDay: false,
            CreatedAt: DateTime.UtcNow);

        var record = ActivityFeedProjector.TryProject(domainEvent, _ => null);

        Assert.NotNull(record);
        Assert.Equal(string.Empty, record!.ActorDisplayName);
        Assert.Equal(nameof(ActivityEventType.CalendarEventCreated), record.EventType);
        Assert.Equal("Team meeting", record.TargetDescription);
    }
}
