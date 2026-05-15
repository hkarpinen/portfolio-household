namespace Infrastructure.Messaging.Events;

/// <summary>Wire shapes for household domain events published via the outbox to RabbitMQ.
/// Must match the flat camelCase JSON produced by OutboxExtensions.AddToOutbox.</summary>
/// 
public sealed record HouseholdCreatedEvent(
    Guid HouseholdId,
    Guid OwnerId,
    string Name,
    string? Description,
    string CurrencyCode,
    DateTime CreatedAt);

public sealed record HouseholdUpdatedEvent(
    Guid HouseholdId,
    string Name,
    string? Description,
    string CurrencyCode,
    DateTime UpdatedAt);

public sealed record HouseholdDeletedEvent(
    Guid HouseholdId,
    DateTime DeletedAt);

public sealed record HouseholdOwnershipTransferredEvent(
    Guid HouseholdId,
    Guid PreviousOwnerId,
    Guid NewOwnerId,
    DateTime TransferredAt);

public sealed record HouseholdMemberJoinedEvent(
    Guid MembershipId,
    Guid HouseholdId,
    Guid UserId,
    string Role,
    DateTime JoinedAt);

public sealed record HouseholdMemberLeftEvent(
    Guid MembershipId,
    Guid HouseholdId,
    Guid UserId,
    DateTime LeftAt);

public sealed record HouseholdMemberRemovedEvent(
    Guid MembershipId,
    Guid HouseholdId,
    Guid RemovedByUserId,
    Guid RemovedUserId,
    DateTime RemovedAt);

public sealed record HouseholdMemberRoleChangedEvent(
    Guid MembershipId,
    Guid HouseholdId,
    Guid UserId,
    string OldRole,
    string NewRole,
    DateTime ChangedAt);

public sealed record HouseholdMemberInvitedEvent(
    Guid MembershipId,
    Guid HouseholdId,
    Guid InvitedByUserId,
    string InvitationCode,
    DateTime InvitedAt);
