using Household.Application.Dtos;
using Household.Domain;
using Household.Domain.Events;

namespace Infrastructure.Persistence;

internal static class ActivityFeedProjector
{
    // Null for events that produce no feed entry. An unresolvable display name falls back to empty
    // rather than blocking the save.
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

            _ => null,
        };
    }
}
