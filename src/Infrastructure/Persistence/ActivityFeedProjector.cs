using Household.Application.Dtos;
using Household.Domain;
using Household.Domain.Events;

namespace Infrastructure.Persistence;

/// <summary>
/// Pure static projection logic: maps a domain event into an <see cref="ActivityEventRecord"/>.
/// Kept stateless so it can be unit-tested without a DbContext and later moved to a
/// MassTransit consumer without rewriting the mapping.
/// </summary>
/// <remarks>
/// Finance cross-service events (<c>ExpenseCreated</c>, <c>ExpenseSplitPaid</c>) arrive over
/// RabbitMQ and are projected by <see cref="Infrastructure.Messaging.Consumers"/>.FinanceConsumers
/// directly — not through this projector — because the consumer needs DbContext access for user
/// lookup and idempotency tracking that this stateless mapper deliberately avoids.
/// </remarks>
internal static class ActivityFeedProjector
{
    /// <summary>
    /// Maps a domain event to an <see cref="ActivityEventRecord"/>.
    /// Returns <c>null</c> for events that do not produce activity feed entries.
    /// </summary>
    /// <param name="domainEvent">The domain event raised by an aggregate.</param>
    /// <param name="resolveDisplayName">
    /// Function that resolves a user ID to a display name.
    /// Returns <c>null</c> if the user projection is not available — the projector falls
    /// back to empty string to avoid blocking the save.
    /// </param>
    public static ActivityEventRecord? TryProject(
        DomainEvent domainEvent,
        Func<Guid, string?> resolveDisplayName)
    {
        return domainEvent switch
        {
            HouseholdMemberJoined e => new ActivityEventRecord
            {
                Id = Guid.NewGuid(),
                HouseholdId = e.HouseholdId,
                EventType = nameof(ActivityEventType.MemberJoined),
                ActorId = e.UserId,
                ActorDisplayName = resolveDisplayName(e.UserId) ?? string.Empty,
                TargetId = e.MembershipId,
                TargetDescription = e.Role,
                OccurredAt = e.JoinedAt,
            },

            HouseholdMemberLeft e => new ActivityEventRecord
            {
                Id = Guid.NewGuid(),
                HouseholdId = e.HouseholdId,
                EventType = nameof(ActivityEventType.MemberLeft),
                ActorId = e.UserId,
                ActorDisplayName = resolveDisplayName(e.UserId) ?? string.Empty,
                TargetId = e.MembershipId,
                TargetDescription = null,
                OccurredAt = e.LeftAt,
            },

            HouseholdMemberRoleChanged e => new ActivityEventRecord
            {
                Id = Guid.NewGuid(),
                HouseholdId = e.HouseholdId,
                EventType = nameof(ActivityEventType.MemberPromoted),
                ActorId = e.UserId,
                ActorDisplayName = resolveDisplayName(e.UserId) ?? string.Empty,
                TargetId = e.MembershipId,
                TargetDescription = $"{e.OldRole} → {e.NewRole}",
                OccurredAt = e.ChangedAt,
            },

            ChoreCreated e => new ActivityEventRecord
            {
                Id = Guid.NewGuid(),
                HouseholdId = e.HouseholdId,
                EventType = nameof(ActivityEventType.ChoreCreated),
                ActorId = e.CreatedByUserId,
                ActorDisplayName = resolveDisplayName(e.CreatedByUserId) ?? string.Empty,
                TargetId = e.ChoreId,
                TargetDescription = e.Title,
                OccurredAt = e.CreatedAt,
            },

            ChoreCompleted e => new ActivityEventRecord
            {
                Id = Guid.NewGuid(),
                HouseholdId = e.HouseholdId,
                EventType = nameof(ActivityEventType.ChoreCompleted),
                ActorId = e.CompletedByUserId,
                ActorDisplayName = resolveDisplayName(e.CompletedByUserId) ?? string.Empty,
                TargetId = e.ChoreId,
                TargetDescription = null,
                OccurredAt = e.CompletedAt,
            },

            CalendarEventCreated e => new ActivityEventRecord
            {
                Id = Guid.NewGuid(),
                HouseholdId = e.HouseholdId,
                EventType = nameof(ActivityEventType.CalendarEventCreated),
                ActorId = e.CreatedByUserId,
                ActorDisplayName = resolveDisplayName(e.CreatedByUserId) ?? string.Empty,
                TargetId = e.CalendarEventId,
                TargetDescription = e.Title,
                OccurredAt = e.CreatedAt,
            },

            // All other domain events (HouseholdCreated, ChoreAssigned, etc.) do not
            // produce activity feed entries and are intentionally skipped.
            _ => null,
        };
    }
}
