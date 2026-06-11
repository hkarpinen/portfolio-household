namespace Infrastructure.Persistence;

/// <summary>
/// Flat read-model row for the household activity feed.
/// This is NOT a domain aggregate — no domain events, no factory methods.
/// Projected from domain events inside DrainDomainEventsToOutbox.
/// </summary>
public sealed class ActivityEventRecord
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    /// <summary>Enum value stored as string for queryability.</summary>
    public string EventType { get; set; } = string.Empty;
    public Guid ActorId { get; set; }
    public string ActorDisplayName { get; set; } = string.Empty;
    public Guid? TargetId { get; set; }
    public string? TargetDescription { get; set; }
    public DateTime OccurredAt { get; set; }
}
