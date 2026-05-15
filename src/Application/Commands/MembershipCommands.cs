using Household.Domain.ValueObjects;

namespace Household.Application.Commands;

public sealed record JoinHouseholdCommand(
    Guid HouseholdId,
    Guid RequestingUserId);

public sealed record InviteMemberCommand(
    Guid HouseholdId,
    Guid RequestingUserId);

public sealed record AcceptInvitationCommand(
    string InvitationCode,
    Guid RequestingUserId);

public sealed record LeaveHouseholdCommand(
    Guid HouseholdId,
    Guid RequestingUserId);

public sealed record RemoveMemberCommand(
    Guid HouseholdId,
    Guid MembershipId,
    Guid RequestingUserId);

public sealed record ChangeMemberRoleCommand(
    Guid HouseholdId,
    Guid MembershipId,
    Guid RequestingUserId,
    HouseholdRole NewRole);
