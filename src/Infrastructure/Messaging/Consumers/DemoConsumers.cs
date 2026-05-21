using Household.Application.Managers.Demo;
using Household.Domain.ValueObjects;
using Infrastructure.Messaging.Events;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Messaging.Consumers;

internal sealed class DemoUserCreatedConsumer(
    HouseholdDbContext db,
    IDemoSeedManager demoSeedManager) : IConsumer<DemoUserCreatedEvent>
{
    public async Task Consume(ConsumeContext<DemoUserCreatedEvent> context)
    {
        var message = context.Message;
        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.Id, context.CancellationToken))
            return;

        // Infrastructure concern: create or update the user projection
        var projection = await db.UserProjections
            .FirstOrDefaultAsync(u => u.Id == message.UserId, context.CancellationToken);

        if (projection is null)
        {
            projection = new UserProjection
            {
                Id = message.UserId,
                Username = BuildUsername(message.Email),
                DisplayName = message.DisplayName,
                CreatedAt = message.OccurredAt,
                UpdatedAt = message.OccurredAt,
                IsDemo = true
            };
            db.UserProjections.Add(projection);
        }
        else
        {
            projection.IsDemo = true;
            projection.UpdatedAt = message.OccurredAt;
        }

        // Application concern: seed domain aggregates via manager
        // SeedAsync is idempotent and commits via the shared DbContext (which also persists the projection above)
        var householdId = await demoSeedManager.SeedAsync(message.UserId, message.DisplayName, context.CancellationToken);

        // Notify the finance service so it can seed shared household bills.
        if (householdId.HasValue)
        {
            db.AddToOutbox(new DemoHouseholdSeededEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                message.UserId,
                householdId.Value));
        }

        // Mark readiness and record idempotency key
        projection.DemoSeedCompletedAt = DateTime.UtcNow;
        db.ProcessedEvents.Add(new ProcessedEvent(message.Id, nameof(DemoUserCreatedEvent), DateTime.UtcNow));

        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }

    private static string BuildUsername(string email)
    {
        if (email.Contains('@'))
        {
            var local = email.Split('@', 2)[0].Trim();
            if (!string.IsNullOrWhiteSpace(local)) return local.ToLowerInvariant();
        }
        return $"demo_{Guid.NewGuid():N}";
    }
}

internal sealed class DemoUserExpiredConsumer(
    HouseholdDbContext db,
    IDemoSeedManager demoSeedManager) : IConsumer<DemoUserExpiredEvent>
{
    public async Task Consume(ConsumeContext<DemoUserExpiredEvent> context)
    {
        var message = context.Message;
        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.Id, context.CancellationToken))
            return;

        // Application concern: delete domain aggregates via manager
        await demoSeedManager.CleanupAsync(message.UserId, context.CancellationToken);

        // Infrastructure concern: remove user projection
        var projection = await db.UserProjections
            .FirstOrDefaultAsync(u => u.Id == message.UserId, context.CancellationToken);
        if (projection is not null)
            db.UserProjections.Remove(projection);

        db.ProcessedEvents.Add(new ProcessedEvent(message.Id, nameof(DemoUserExpiredEvent), DateTime.UtcNow));

        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }
}
