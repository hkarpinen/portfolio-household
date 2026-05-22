using Domain.Events;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Messaging.Consumers;

internal sealed class UserRegisteredConsumer(HouseholdDbContext db) : IConsumer<UserRegistered>
{
    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var message = context.Message;
        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.Id, context.CancellationToken))
            return;

        var existing = await db.UserProjections.FirstOrDefaultAsync(u => u.Id == message.UserId, context.CancellationToken);
        if (existing is null)
        {
            db.UserProjections.Add(new UserProjection
            {
                Id = message.UserId,
                Username = BuildUsername(message.Email, message.DisplayName, message.UserId),
                DisplayName = message.DisplayName,
                CreatedAt = message.OccurredAt,
                UpdatedAt = message.OccurredAt
            });
        }
        else
        {
            existing.Username = BuildUsername(message.Email, message.DisplayName, message.UserId);
            existing.DisplayName = message.DisplayName;
            existing.UpdatedAt = message.OccurredAt;
        }

        db.ProcessedEvents.Add(new ProcessedEvent(message.Id, nameof(UserRegistered), DateTime.UtcNow));
        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }

    private static string BuildUsername(string? email, string? displayName, Guid userId)
    {
        if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
        {
            var local = email.Split('@', 2)[0].Trim();
            if (!string.IsNullOrWhiteSpace(local)) return local.ToLowerInvariant();
        }
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName.Trim().ToLowerInvariant().Replace(' ', '_');
        return $"user_{userId:N}";
    }
}

internal sealed class UserProfileUpdatedConsumer(HouseholdDbContext db) : IConsumer<UserProfileUpdated>
{
    public async Task Consume(ConsumeContext<UserProfileUpdated> context)
    {
        var message = context.Message;
        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.Id, context.CancellationToken))
            return;

        var existing = await db.UserProjections.FirstOrDefaultAsync(u => u.Id == message.UserId, context.CancellationToken);
        if (existing is not null)
        {
            if (message.DisplayName is not null) existing.DisplayName = message.DisplayName;
            if (message.AvatarUrl is not null) existing.AvatarUrl = message.AvatarUrl;
            existing.UpdatedAt = message.OccurredAt;
        }

        db.ProcessedEvents.Add(new ProcessedEvent(message.Id, nameof(UserProfileUpdated), DateTime.UtcNow));
        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }
}
