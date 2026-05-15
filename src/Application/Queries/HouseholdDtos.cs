using Household.Domain.ValueObjects;

namespace Household.Application.Queries;

public sealed record HouseholdDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OwnerId,
    string CurrencyCode,
    DateTime CreatedAt,
    int MemberCount);

public sealed record HouseholdSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    string CurrencyCode,
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
    DateTime JoinedAt);
