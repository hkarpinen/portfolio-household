using Household.Domain.ValueObjects;

namespace Household.Application.Queries;

public sealed record HouseholdDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    string CurrencyCode,
    string Timezone,
    DateTime CreatedAt,
    int MemberCount);

public sealed record HouseholdSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    string CurrencyCode,
    string Timezone,
    HouseholdRole Role,
    DateTime JoinedAt,
    int MemberCount,
    DateTime CreatedAt);

public sealed record MemberDto(
    Guid MembershipId,
    Guid UserId,
    string Username,
    string? DisplayName,
    HouseholdRole Role,
    DateTime JoinedAt,
    string? PendingInvitationCode);

/// <summary>
/// What an invite code resolves to WITHOUT spending it. Deliberately thin: anyone holding a
/// code can read this, so it carries enough to recognise the place and nothing more — no
/// member names, no money, no id of anyone already in.
/// </summary>
public sealed record InvitationPreviewDto(
    Guid HouseholdId,
    string HouseholdName,
    int MemberCount,
    DateTime? ExpiresAt,
    bool HasExpired);
