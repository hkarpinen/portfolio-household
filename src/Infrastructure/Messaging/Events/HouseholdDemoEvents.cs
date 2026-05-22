namespace Infrastructure.Messaging.Events;

/// <summary>Published by the household service after the demo household is fully seeded.
/// Finance service consumes this to seed shared household bills.</summary>
public sealed record DemoHouseholdSeededEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid UserId,
    Guid HouseholdId);
