using Household.Domain.ValueObjects;

namespace Household.Application.Queries;

/// <summary>
/// Single calendar entry as returned by the calendar query. `Kind` discriminates
/// member-authored entries from finance-bill mirrors; the latter additionally
/// carry `LinkedExpenseId` so the UI can deep-link to the expense detail and
/// suppress edit/delete affordances (the backend also rejects edits server-side).
/// For recurring entries the query expands each rule into one DTO per occurrence
/// inside the requested window — clients render a flat list. The rule itself
/// (`RecurrenceFrequency` + `RecurrenceEndDate`) is repeated on every occurrence
/// so the edit form can pre-fill from any visible cell.
/// </summary>
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
