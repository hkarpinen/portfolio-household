namespace Household.Application.Dtos;

public enum ActivityEventType
{
    MemberJoined,
    MemberLeft,
    MemberPromoted,
    ChoreCreated,
    ChoreCompleted,
    CalendarEventCreated,
    // Both are cross-service: populated only when finance events arrive.
    ExpenseCreated,
    SplitPaid
}

public sealed record ActivityEventDto(
    Guid EventId,
    ActivityEventType EventType,
    Guid ActorId,
    string ActorDisplayName,
    Guid? TargetId,
    string? TargetDescription,
    DateTime OccurredAt);

public sealed record ActivityFeedListDto(
    IReadOnlyList<ActivityEventDto> Items,
    int TotalCount);
