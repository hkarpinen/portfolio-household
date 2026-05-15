using System.Text.Json;
using System.Text.Json.Serialization;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence.Outbox;

namespace Infrastructure.Persistence;

internal sealed class HouseholdIdConverter : JsonConverter<HouseholdId>
{
    public override HouseholdId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, HouseholdId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

internal sealed class MembershipIdConverter : JsonConverter<MembershipId>
{
    public override MembershipId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, MembershipId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

internal sealed class ChoreIdConverter : JsonConverter<ChoreId>
{
    public override ChoreId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, ChoreId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

internal sealed class CalendarEventIdConverter : JsonConverter<CalendarEventId>
{
    public override CalendarEventId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, CalendarEventId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

internal sealed class UserIdConverter : JsonConverter<UserId>
{
    public override UserId Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) => new(r.GetGuid());
    public override void Write(Utf8JsonWriter w, UserId v, JsonSerializerOptions o) => w.WriteStringValue(v.Value);
}

internal static class OutboxExtensions
{
    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new HouseholdIdConverter(),
            new MembershipIdConverter(),
            new ChoreIdConverter(),
            new CalendarEventIdConverter(),
            new UserIdConverter()
        }
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
