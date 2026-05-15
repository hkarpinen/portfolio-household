namespace Household.Domain.ValueObjects;

public readonly record struct HouseholdId(Guid Value)
{
    public static HouseholdId New() => new(Guid.NewGuid());
    public static HouseholdId Create(Guid value) => new(value);
}

public readonly record struct MembershipId(Guid Value)
{
    public static MembershipId New() => new(Guid.NewGuid());
    public static MembershipId Create(Guid value) => new(value);
}

public readonly record struct ChoreId(Guid Value)
{
    public static ChoreId New() => new(Guid.NewGuid());
    public static ChoreId Create(Guid value) => new(value);
}

public readonly record struct CalendarEventId(Guid Value)
{
    public static CalendarEventId New() => new(Guid.NewGuid());
    public static CalendarEventId Create(Guid value) => new(value);
}

public readonly record struct UserId(Guid Value)
{
    public static UserId Create(Guid value) => new(value);
}

public enum HouseholdRole { Member, Admin, Owner }

public enum RecurrenceFrequency { Daily, Weekly, BiWeekly, Monthly }
