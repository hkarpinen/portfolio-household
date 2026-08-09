using Finance.Domain.Events;
using Household.Application.Dtos;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Messaging.Consumers;

// Actor names are resolved from a local projection — this service never queries
// the publisher. Calendar rows are keyed `(Source, LinkedExpenseId)` and upserted,
// so redelivery is harmless.

internal sealed class ChargeCreatedConsumer(HouseholdDbContext db) : IConsumer<ChargeCreated>
{
    public async Task Consume(ConsumeContext<ChargeCreated> context)
    {
        var message = context.Message;

        // Personal expenses arrive on the same event with no household — skip them.
        if (message.GroupId is null)
            return;

        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.EventId, context.CancellationToken))
            return;

        var actorDisplayName = await db.UserProjections
            .Where(u => u.Id == message.UserId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(context.CancellationToken);

        db.ActivityEvents.Add(new ActivityEventRecord
        {
            Id = Guid.NewGuid(),
            HouseholdId = message.GroupId.Value,
            EventType = nameof(ActivityEventType.ExpenseCreated),
            ActorId = message.UserId,
            ActorDisplayName = actorDisplayName ?? string.Empty,
            TargetId = message.ChargeId,
            TargetDescription = message.Title,
            OccurredAt = message.OccurredAt,
        });

        var existing = await db.CalendarEvents
            .FirstOrDefaultAsync(
                e => e.Source == CalendarEventSource.FinanceBill && e.LinkedExpenseId == message.ChargeId,
                context.CancellationToken);

        if (existing is null)
        {
            var entry = HouseholdCalendarEvent.CreateFromBill(
                HouseholdId.Create(message.GroupId.Value),
                UserId.Create(message.UserId),
                message.ChargeId,
                message.Title,
                DateTime.SpecifyKind(message.DueDate, DateTimeKind.Utc),
                ParseFrequency(message.RecurrenceSchedule?.Frequency),
                message.RecurrenceSchedule?.EndDate is { } end ? DateTime.SpecifyKind(end, DateTimeKind.Utc) : null);
            db.CalendarEvents.Add(entry);
        }
        else
        {
            // Replay or out-of-order: take the latest snapshot and resurrect a
            // soft-hidden row.
            existing.UpdateFromBill(
                message.Title,
                DateTime.SpecifyKind(message.DueDate, DateTimeKind.Utc),
                ParseFrequency(message.RecurrenceSchedule?.Frequency),
                message.RecurrenceSchedule?.EndDate is { } end2 ? DateTime.SpecifyKind(end2, DateTimeKind.Utc) : null);
        }

        db.ProcessedEvents.Add(new ProcessedEvent(message.EventId, nameof(ChargeCreated), DateTime.UtcNow));
        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Concurrent delivery — another instance already wrote the processed_events row.
        }
    }

    internal static RecurrenceFrequency? ParseFrequency(string? wire) =>
        string.IsNullOrEmpty(wire) ? null
        : Enum.TryParse<RecurrenceFrequency>(wire, ignoreCase: true, out var f) ? f
        : null;
}

internal sealed class ChargeUpdatedConsumer(HouseholdDbContext db) : IConsumer<ChargeUpdated>
{
    public async Task Consume(ConsumeContext<ChargeUpdated> context)
    {
        var message = context.Message;
        if (message.GroupId is null) return;

        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.EventId, context.CancellationToken))
            return;

        var existing = await db.CalendarEvents
            .FirstOrDefaultAsync(
                e => e.Source == CalendarEventSource.FinanceBill && e.LinkedExpenseId == message.ChargeId,
                context.CancellationToken);

        if (existing is not null)
        {
            existing.UpdateFromBill(
                message.Title,
                DateTime.SpecifyKind(message.DueDate, DateTimeKind.Utc),
                ChargeCreatedConsumer.ParseFrequency(message.RecurrenceSchedule?.Frequency),
                message.RecurrenceSchedule?.EndDate is { } end ? DateTime.SpecifyKind(end, DateTimeKind.Utc) : null);
        }
        // An Update alone cannot reconstruct CreatedBy, so a missed Created is left
        // for the next one to seed rather than guessed at.

        db.ProcessedEvents.Add(new ProcessedEvent(message.EventId, nameof(ChargeUpdated), DateTime.UtcNow));
        try { await db.SaveChangesAsync(context.CancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }
}

internal sealed class ChargeDeactivatedConsumer(HouseholdDbContext db) : IConsumer<ChargeDeactivated>
{
    public async Task Consume(ConsumeContext<ChargeDeactivated> context)
    {
        var message = context.Message;
        if (message.GroupId is null) return;

        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.EventId, context.CancellationToken))
            return;

        var existing = await db.CalendarEvents
            .FirstOrDefaultAsync(
                e => e.Source == CalendarEventSource.FinanceBill && e.LinkedExpenseId == message.ChargeId,
                context.CancellationToken);

        if (existing is not null && existing.DeletedAt is null)
            existing.DeactivateFromBill();

        db.ProcessedEvents.Add(new ProcessedEvent(message.EventId, nameof(ChargeDeactivated), DateTime.UtcNow));
        try { await db.SaveChangesAsync(context.CancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }
}

internal sealed class ChargeActivatedConsumer(HouseholdDbContext db) : IConsumer<ChargeActivated>
{
    public async Task Consume(ConsumeContext<ChargeActivated> context)
    {
        var message = context.Message;
        if (message.GroupId is null) return;

        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.EventId, context.CancellationToken))
            return;

        var existing = await db.CalendarEvents
            .FirstOrDefaultAsync(
                e => e.Source == CalendarEventSource.FinanceBill && e.LinkedExpenseId == message.ChargeId,
                context.CancellationToken);

        if (existing is not null && existing.DeletedAt is not null)
            existing.ActivateFromBill();

        db.ProcessedEvents.Add(new ProcessedEvent(message.EventId, nameof(ChargeActivated), DateTime.UtcNow));
        try { await db.SaveChangesAsync(context.CancellationToken); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { }
    }
}

internal sealed class SettlementRecordedConsumer(HouseholdDbContext db) : IConsumer<SettlementRecorded>
{
    public async Task Consume(ConsumeContext<SettlementRecorded> context)
    {
        var message = context.Message;

        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == message.EventId, context.CancellationToken))
            return;

        var actorDisplayName = await db.UserProjections
            .Where(u => u.Id == message.FromUserId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(context.CancellationToken);

        db.ActivityEvents.Add(new ActivityEventRecord
        {
            Id = Guid.NewGuid(),
            HouseholdId = message.GroupId,
            EventType = nameof(ActivityEventType.SplitPaid),
            ActorId = message.FromUserId,
            ActorDisplayName = actorDisplayName ?? string.Empty,
            // TargetDescription stays null: the event carries no title, and there is
            // no local read model to look one up in.
            TargetId = message.AllocationId,
            TargetDescription = null,
            OccurredAt = message.OccurredAt,
        });

        db.ProcessedEvents.Add(new ProcessedEvent(message.EventId, nameof(SettlementRecorded), DateTime.UtcNow));
        try
        {
            await db.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Concurrent delivery — another instance already wrote the processed_events row.
        }
    }
}
