using Household.Domain.ValueObjects;

namespace Household.Domain.Events;

public sealed record HouseholdCreated(
    HouseholdId HouseholdId,
    UserId OwnerId,
    string Name,
    string? Description,
    string CurrencyCode,
    DateTime CreatedAt) : DomainEvent;

public sealed record HouseholdUpdated(
    HouseholdId HouseholdId,
    string Name,
    string? Description,
    string CurrencyCode,
    DateTime UpdatedAt) : DomainEvent;

public sealed record HouseholdOwnershipTransferred(
    HouseholdId HouseholdId,
    UserId PreviousOwnerId,
    UserId NewOwnerId,
    DateTime TransferredAt) : DomainEvent;

public sealed record HouseholdDeleted(
    HouseholdId HouseholdId,
    DateTime DeletedAt) : DomainEvent;

public sealed record HouseholdMemberInvited(
    MembershipId MembershipId,
    HouseholdId HouseholdId,
    UserId InvitedByUserId,
    string InvitationCode,
    DateTime InvitedAt) : DomainEvent;

public sealed record HouseholdMemberJoined(
    MembershipId MembershipId,
    HouseholdId HouseholdId,
    UserId UserId,
    HouseholdRole Role,
    DateTime JoinedAt) : DomainEvent;

public sealed record HouseholdMemberLeft(
    MembershipId MembershipId,
    HouseholdId HouseholdId,
    UserId UserId,
    DateTime LeftAt) : DomainEvent;

public sealed record HouseholdMemberRemoved(
    MembershipId MembershipId,
    HouseholdId HouseholdId,
    UserId RemovedByUserId,
    UserId RemovedUserId,
    DateTime RemovedAt) : DomainEvent;

public sealed record HouseholdMemberRoleChanged(
    MembershipId MembershipId,
    HouseholdId HouseholdId,
    UserId UserId,
    HouseholdRole OldRole,
    HouseholdRole NewRole,
    DateTime ChangedAt) : DomainEvent;
