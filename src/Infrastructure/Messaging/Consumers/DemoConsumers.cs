using Domain.Events;
using Household.Application.Managers.Demo;
using Infrastructure.Messaging.Events;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Messaging.Consumers;

internal sealed class DemoUserCreatedConsumer(
    HouseholdDbContext db,
    IDemoSeedManager demoSeedManager) : IConsumer<DemoUserCreated>
{
    public async Task Consume(ConsumeContext<DemoUserCreated> context)
    {
        var message = context.Message;

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

        var householdId = await demoSeedManager.SeedAsync(message.UserId, message.DisplayName, context.CancellationToken);

        if (householdId.HasValue)
        {
            // Publishing from inside a consumer with the outbox configured writes to the outbox
            // in this same transaction rather than going straight to the broker.
            await context.Publish(new DemoHouseholdSeededEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                message.UserId,
                householdId.Value), context.CancellationToken);
        }

        projection.DemoSeedCompletedAt = DateTime.UtcNow;

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
    IDemoSeedManager demoSeedManager) : IConsumer<DemoUserExpired>
{
    public async Task Consume(ConsumeContext<DemoUserExpired> context)
    {
        var message = context.Message;

        await demoSeedManager.CleanupAsync(message.UserId, context.CancellationToken);

        var projection = await db.UserProjections
            .FirstOrDefaultAsync(u => u.Id == message.UserId, context.CancellationToken);
        if (projection is not null)
            db.UserProjections.Remove(projection);

        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }
}
