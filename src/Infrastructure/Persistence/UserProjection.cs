namespace Infrastructure.Persistence.Outbox;

// Namespace kept as-is: this shared a file with the hand-rolled outbox models, and moving it would
// touch every consumer that reads it for no gain.
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
