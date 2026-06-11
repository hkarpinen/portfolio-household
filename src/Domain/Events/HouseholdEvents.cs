namespace Household.Domain.Events;

public sealed record HouseholdCreated(
    Guid HouseholdId,
    Guid OwnerId,
    string Name,
    string? Description,
    string CurrencyCode,
    DateTime CreatedAt) : DomainEvent;

public sealed record HouseholdUpdated(
    Guid HouseholdId,
    string Name,
    string? Description,
    string CurrencyCode,
    DateTime UpdatedAt) : DomainEvent;

public sealed record HouseholdOwnershipTransferred(
    Guid HouseholdId,
    Guid PreviousOwnerId,
    Guid NewOwnerId,
    DateTime TransferredAt) : DomainEvent;

public sealed record HouseholdDeleted(
    Guid HouseholdId,
    DateTime DeletedAt) : DomainEvent;

public sealed record HouseholdMemberInvited(
    Guid MembershipId,
    Guid HouseholdId,
    string HouseholdName,
    Guid InvitedByUserId,
    string InvitationCode,
    string? RecipientEmail,
    DateTime InvitedAt) : DomainEvent;

public sealed record HouseholdMemberJoined(
    Guid MembershipId,
    Guid HouseholdId,
    Guid UserId,
    string Role,
    DateTime JoinedAt) : DomainEvent;

public sealed record HouseholdMemberLeft(
    Guid MembershipId,
    Guid HouseholdId,
    Guid UserId,
    DateTime LeftAt) : DomainEvent;

public sealed record HouseholdMemberRemoved(
    Guid MembershipId,
    Guid HouseholdId,
    Guid RemovedByUserId,
    Guid RemovedUserId,
    DateTime RemovedAt) : DomainEvent;

public sealed record HouseholdMemberRoleChanged(
    Guid MembershipId,
    Guid HouseholdId,
    Guid UserId,
    string OldRole,
    string NewRole,
    DateTime ChangedAt) : DomainEvent;

/// <summary>
/// Household has authorized (role-checked) a member's allocation — their share — on a finance
/// charge. Finance consumes this to upsert the allocation; the <c>UserId</c> here is authoritative
/// (household already verified the caller may act for that member). This is how a role-gated
/// "add a split for another member" reaches finance without any service-to-service call, and
/// without finance learning household roles. <c>GroupId</c> == the household id (same opaque Guid).
/// </summary>
public sealed record GroupAllocationAssigned(
    Guid Id,
    DateTime OccurredAt,
    Guid GroupId,
    Guid ChargeId,
    Guid UserId,
    decimal Amount,
    string Currency) : DomainEvent;
