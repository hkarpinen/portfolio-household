namespace Household.Application.Commands;

public sealed record CreateHouseholdCommand(
    Guid RequestingUserId,
    string Name,
    string? Description,
    string CurrencyCode);

public sealed record UpdateHouseholdCommand(
    Guid HouseholdId,
    Guid RequestingUserId,
    string Name,
    string? Description,
    string CurrencyCode);

public sealed record TransferOwnershipCommand(
    Guid HouseholdId,
    Guid RequestingUserId,
    Guid NewOwnerId);

public sealed record DeleteHouseholdCommand(
    Guid HouseholdId,
    Guid RequestingUserId);
