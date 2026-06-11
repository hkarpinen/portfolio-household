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

/// <summary>
/// Shared with finance's `RecurrenceFrequency` (same int values, same names) so a bill
/// recurrence consumed off the wire round-trips into household without remapping.
/// </summary>
public enum RecurrenceFrequency
{
    Daily = 0,
    Weekly = 1,
    BiWeekly = 2,
    Monthly = 3,
    Quarterly = 4,
    SemiAnnually = 5,
    Annually = 6
}

/// <summary>
/// Discriminates who/what authored a calendar event. `Member` is a user-created
/// entry; `FinanceBill` is mirrored from a finance shared-expense event and is
/// read-only on this side — `Update`/`Delete` reject non-`Member` sources so the
/// API can 403 cleanly and finance stays the source of truth.
/// </summary>
public enum CalendarEventSource { Member = 0, FinanceBill = 1 }
