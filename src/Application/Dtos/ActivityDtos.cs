namespace Household.Application.Dtos;

public enum ActivityEventType
{
    MemberJoined,
    MemberLeft,
    MemberPromoted,
    ChoreCreated,
    ChoreCompleted,
    CalendarEventCreated,
    ExpenseCreated,  // cross-service (finance) — populated when finance events arrive
    SplitPaid        // cross-service (finance) — matches finance's ExpenseSplitPaid event name
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
