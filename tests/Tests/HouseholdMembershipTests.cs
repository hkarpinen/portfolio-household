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
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), NewUserId());

        Assert.False(membership.IsActive);
        Assert.NotNull(membership.InvitationCode);
        Assert.Equal(8, membership.InvitationCode!.Length);
        Assert.Equal(membership.InvitationCode, membership.InvitationCode.ToUpperInvariant());
    }

    [Fact]
    public void CreateWithInvitation_RaisesHouseholdMemberInvitedEvent()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), NewUserId());

        Assert.Single(membership.DomainEvents);
        Assert.IsType<HouseholdMemberInvited>(membership.DomainEvents.First());
    }

    [Fact]
    public void AcceptInvitation_ActivatesMembershipWithUserId()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), NewUserId());
        membership.ClearDomainEvents();
        var joiningUser = NewUserId();

        membership.AcceptInvitation(joiningUser);

        Assert.True(membership.IsActive);
        Assert.Equal(joiningUser, membership.UserId);
    }

    [Fact]
    public void AcceptInvitation_RaisesHouseholdMemberJoinedEvent()
    {
        var membership = HouseholdMembership.CreateWithInvitation(NewHouseholdId(), NewUserId());
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
