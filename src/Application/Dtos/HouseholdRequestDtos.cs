using Household.Domain.ValueObjects;

namespace Household.Application.Dtos;

public sealed record CreateHouseholdRequest(string Name, string? Description, string CurrencyCode, string? Timezone = null);
public sealed record UpdateHouseholdRequest(string Name, string? Description, string CurrencyCode, string? Timezone = null);
public sealed record InviteRequest(string? RecipientEmail);
public sealed record TransferOwnershipRequest(Guid NewOwnerId);
public sealed record AcceptInvitationRequest(string InvitationCode);
public sealed record ChangeMemberRoleRequest(HouseholdRole Role);
public sealed record AssignAllocationRequest(Guid UserId, decimal Amount, string Currency);
