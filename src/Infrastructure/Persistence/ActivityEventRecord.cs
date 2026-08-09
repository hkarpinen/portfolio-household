namespace Infrastructure.Persistence;

public sealed class ActivityEventRecord
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    // Stored as a string, not an int, so the column stays queryable.
    public string EventType { get; set; } = string.Empty;
    public Guid ActorId { get; set; }
    public string ActorDisplayName { get; set; } = string.Empty;
    public Guid? TargetId { get; set; }
    public string? TargetDescription { get; set; }
    public DateTime OccurredAt { get; set; }
}
