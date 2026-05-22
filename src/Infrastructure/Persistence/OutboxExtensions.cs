using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Persistence.Outbox;

namespace Infrastructure.Persistence;

internal static class OutboxExtensions
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static void AddToOutbox(this HouseholdDbContext context, object domainEvent)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions),
            CreatedAt = DateTime.UtcNow,
            Published = false
        };
        context.OutboxMessages.Add(message);
    }
}
