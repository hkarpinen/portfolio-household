using Household.Domain.Events;
using Household.Domain.ValueObjects;

namespace Household.Domain.Aggregates;

public sealed class HouseholdMembership : IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];

    public MembershipId Id { get; private set; }
    public HouseholdId HouseholdId { get; private set; }
    public UserId UserId { get; private set; }
    public HouseholdRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public string? InvitationCode { get; private set; }

    private HouseholdMembership() { }

    public static HouseholdMembership Create(HouseholdId householdId, UserId userId, HouseholdRole role)
    {
        var now = DateTime.UtcNow;
        var membership = new HouseholdMembership
        {
            Id = MembershipId.New(),
            HouseholdId = householdId,
            UserId = userId,
            Role = role,
            JoinedAt = now,
            UpdatedAt = now,
            IsActive = true
        };
        membership._domainEvents.Add(new HouseholdMemberJoined(
            membership.Id, householdId, userId, role, now));
        return membership;
    }

    public static HouseholdMembership CreateWithInvitation(HouseholdId householdId, UserId invitedByUserId)
    {
        var now = DateTime.UtcNow;
        var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var membership = new HouseholdMembership
        {
            Id = MembershipId.New(),
            HouseholdId = householdId,
            UserId = default,
            Role = HouseholdRole.Member,
            JoinedAt = now,
            UpdatedAt = now,
            IsActive = false,
            InvitationCode = code
        };
        membership._domainEvents.Add(new HouseholdMemberInvited(
            membership.Id, householdId, invitedByUserId, code, now));
        return membership;
    }

    public void AcceptInvitation(UserId userId)
    {
        UserId = userId;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdMemberJoined(Id, HouseholdId, userId, Role, UpdatedAt));
    }

    public void ChangeRole(HouseholdRole newRole)
    {
        var old = Role;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdMemberRoleChanged(Id, HouseholdId, UserId, old, newRole, UpdatedAt));
    }

    public void Leave()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdMemberLeft(Id, HouseholdId, UserId, UpdatedAt));
    }

    public void Remove(UserId removedByUserId)
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new HouseholdMemberRemoved(Id, HouseholdId, removedByUserId, UserId, UpdatedAt));
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}
