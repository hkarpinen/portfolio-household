using Household.Domain.ValueObjects;

namespace Household.Application.Queries;

// `Kind` discriminates member-authored entries from bill mirrors; mirrors additionally carry
// `LinkedExpenseId`, and edits to them are rejected server-side.
//
// A recurring entry is expanded into one DTO per occurrence inside the requested window, so
// clients render a flat list. The rule itself (RecurrenceFrequency + RecurrenceEndDate) is
// repeated on EVERY occurrence so an edit form can pre-fill from any visible cell.
public sealed record CalendarEventDto(
    Guid Id,
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool AllDay,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    CalendarEventKind Kind = CalendarEventKind.Member,
    Guid? LinkedExpenseId = null,
    RecurrenceFrequency? RecurrenceFrequency = null,
    DateTime? RecurrenceEndDate = null);

public enum CalendarEventKind { Member, FinanceBill }
