using Household.Domain.Aggregates;
using Household.Domain.Events;
using Household.Domain.ValueObjects;

namespace Tests;

public class HouseholdMembershipTests
{
    private static UserId NewUserId() => UserId.Create(Guid.NewGuid());
    private static HouseholdId NewHouseholdId() => HouseholdId.New();

    [Fact]
    public void Create_SetsProperties()
    {
        var householdId = NewHouseholdId();
        var userId = NewUserId();

        var membership = HouseholdMembership.Create(householdId, userId, HouseholdRole.Admin);

        Assert.Equal(householdId, membership.HouseholdId);
        Assert.Equal(userId, membership.UserId);
        Assert.Equal(HouseholdRole.Admin, membership.Role);
        Assert.True(membership.IsActive);
        Assert.Null(membership.InvitationCode);
    }

    [Fact]
    public void Create_RaisesHouseholdMemberJoinedEvent()
    {
        var membership = HouseholdMembership.Create(NewHouseholdId(), NewUserId(), HouseholdRole.Member);

        Assert.Single(membership.DomainEvents);
        Assert.IsType<HouseholdMemberJoined>(membership.DomainEvents.First());
    }

    [Fact]
    public void CreateWithInvitation_SetsInactiveWithCode()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), "Test Household", NewUserId());

        Assert.False(membership.IsActive);
        Assert.NotNull(membership.InvitationCode);
        Assert.Equal(8, membership.InvitationCode!.Length);
        Assert.Equal(membership.InvitationCode, membership.InvitationCode.ToUpperInvariant());
    }

    [Fact]
    public void CreateWithInvitation_RaisesHouseholdMemberInvitedEvent()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), "Test Household", NewUserId());

        Assert.Single(membership.DomainEvents);
        Assert.IsType<HouseholdMemberInvited>(membership.DomainEvents.First());
    }

    [Fact]
    public void CreateWithInvitation_GivesTheCodeAWeek()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), "Test Household", NewUserId());

        Assert.NotNull(membership.InvitationExpiresAt);
        var days = (membership.InvitationExpiresAt!.Value - DateTime.UtcNow).TotalDays;
        Assert.InRange(days, 6.9, 7.0);
        Assert.False(membership.InvitationHasExpired(DateTime.UtcNow));
    }

    [Fact]
    public void InvitationHasExpired_IsTrue_PastTheDeadline()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), "Test Household", NewUserId());

        Assert.True(membership.InvitationHasExpired(DateTime.UtcNow.AddDays(8)));
    }

    [Fact]
    public void AcceptInvitation_Throws_WhenTheCodeHasExpired()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), "Test Household", NewUserId());
        // Reach past the setter the way a row loaded from an old database would.
        typeof(HouseholdMembership)
            .GetProperty(nameof(HouseholdMembership.InvitationExpiresAt))!
            .SetValue(membership, DateTime.UtcNow.AddDays(-1));

        Assert.Throws<InvalidOperationException>(() => membership.AcceptInvitation(NewUserId()));
    }

    [Fact]
    public void AcceptInvitation_ActivatesMembershipWithUserId()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), "Test Household", NewUserId());
        membership.ClearDomainEvents();
        var joiningUser = NewUserId();

        membership.AcceptInvitation(joiningUser);

        Assert.True(membership.IsActive);
        Assert.Equal(joiningUser, membership.UserId);
    }

    [Fact]
    public void AcceptInvitation_RaisesHouseholdMemberJoinedEvent()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), "Test Household", NewUserId());
        membership.ClearDomainEvents();

        membership.AcceptInvitation(NewUserId());

        Assert.Single(membership.DomainEvents);
        Assert.IsType<HouseholdMemberJoined>(membership.DomainEvents.First());
    }

    [Fact]
    public void ChangeRole_UpdatesRole()
    {
        var membership = HouseholdMembership.Create(NewHouseholdId(), NewUserId(), HouseholdRole.Member);
        membership.ClearDomainEvents();

        membership.ChangeRole(HouseholdRole.Admin);

        Assert.Equal(HouseholdRole.Admin, membership.Role);
    }

    [Fact]
    public void ChangeRole_RaisesHouseholdMemberRoleChangedEvent()
    {
        var membership = HouseholdMembership.Create(NewHouseholdId(), NewUserId(), HouseholdRole.Member);
        membership.ClearDomainEvents();

        membership.ChangeRole(HouseholdRole.Admin);

        Assert.Single(membership.DomainEvents);
        Assert.IsType<HouseholdMemberRoleChanged>(membership.DomainEvents.First());
    }

    [Fact]
    public void Leave_DeactivatesMembership()
    {
        var membership = HouseholdMembership.Create(NewHouseholdId(), NewUserId(), HouseholdRole.Member);
        membership.ClearDomainEvents();

        membership.Leave();

        Assert.False(membership.IsActive);
    }

    [Fact]
    public void Leave_RaisesHouseholdMemberLeftEvent()
    {
        var membership = HouseholdMembership.Create(NewHouseholdId(), NewUserId(), HouseholdRole.Member);
        membership.ClearDomainEvents();

        membership.Leave();

        Assert.Single(membership.DomainEvents);
        Assert.IsType<HouseholdMemberLeft>(membership.DomainEvents.First());
    }

    [Fact]
    public void Remove_DeactivatesMembership()
    {
        var membership = HouseholdMembership.Create(NewHouseholdId(), NewUserId(), HouseholdRole.Member);
        membership.ClearDomainEvents();

        membership.Remove(NewUserId());

        Assert.False(membership.IsActive);
    }

    [Fact]
    public void Remove_RaisesHouseholdMemberRemovedEvent()
    {
        var membership = HouseholdMembership.Create(NewHouseholdId(), NewUserId(), HouseholdRole.Member);
        membership.ClearDomainEvents();

        membership.Remove(NewUserId());

        Assert.Single(membership.DomainEvents);
        Assert.IsType<HouseholdMemberRemoved>(membership.DomainEvents.First());
    }
}
