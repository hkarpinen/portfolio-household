namespace Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public const int MaxRetryCount = 5;

    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool Published { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public bool DeadLettered { get; set; }
}

public sealed class ProcessedEvent
{
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }

    private ProcessedEvent() { }

    public ProcessedEvent(Guid eventId, string eventType, DateTime processedAt)
    {
        EventId = eventId;
        EventType = eventType;
        ProcessedAt = processedAt;
    }
}

public sealed class UserProjection
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDemo { get; set; }
    public DateTime? DemoSeedCompletedAt { get; set; }
}
