namespace Infrastructure.Messaging.Events;

/// <summary>Wire shapes for identity events consumed from RabbitMQ.
/// Must match the wire shape published by the Identity service outbox.</summary>
/// 
public sealed record UserRegisteredEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid UserId,
    string Email,
    string DisplayName);

public sealed record UserProfileUpdatedEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid UserId,
    string? DisplayName,
    string? AvatarUrl);
