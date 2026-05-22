// Wire contract for identity demo events consumed from RabbitMQ.
// Namespace and type names must match the domain events published by the identity service.
namespace Domain.Events;

public sealed record DemoUserCreated(
    Guid Id,
    DateTime OccurredAt,
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime DemoExpiresAt);

public sealed record DemoUserExpired(
    Guid Id,
    DateTime OccurredAt,
    Guid UserId);
