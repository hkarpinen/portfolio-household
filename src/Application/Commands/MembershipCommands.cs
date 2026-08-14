using Household.Domain.ValueObjects;

namespace Household.Application.Commands;

public sealed record JoinHouseholdCommand(
    Guid HouseholdId,
    Guid RequestingUserId);

public sealed record InviteMemberCommand(
    Guid HouseholdId,
    Guid RequestingUserId,
    string? RecipientEmail = null);

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

/// <summary>Assign a member's share (their share) on a finance expense. A member may assign
/// their OWN share; assigning another member's share requires Owner/Admin. Household authorizes,
/// then emits <c>GroupShareAssigned</c> for finance to apply — no service-to-service call.</summary>
public sealed record AssignShareCommand(
    Guid HouseholdId,
    Guid ExpenseId,
    Guid RequestingUserId,
    Guid TargetUserId,
    decimal Amount,
    string Currency);
