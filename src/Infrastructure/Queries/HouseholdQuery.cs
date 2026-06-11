using Household.Application.Queries;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Queries;

internal sealed class HouseholdQuery(HouseholdDbContext db) : IHouseholdQuery
{
    public async Task<HouseholdDetailDto?> GetHouseholdAsync(Guid householdId, CancellationToken ct = default)
    {
        var hid = HouseholdId.Create(householdId);
        var household = await db.Households
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == hid && h.IsActive, ct);

        if (household is null) return null;

        var memberCount = await db.Memberships
            .CountAsync(m => m.HouseholdId == hid && m.IsActive, ct);

        return new HouseholdDetailDto(
            household.Id.Value,
            household.Name,
            household.Description,
            household.OwnerId.Value,
            household.CurrencyCode,
            household.Timezone,
            household.CreatedAt,
            memberCount);
    }

    public async Task<IReadOnlyList<HouseholdSummaryDto>> ListUserHouseholdsAsync(Guid userId, CancellationToken ct = default)
    {
        var uid = UserId.Create(userId);
        var memberships = await db.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == uid && m.IsActive)
            .ToListAsync(ct);

        var householdIds = memberships.Select(m => m.HouseholdId).ToList();
        var households = await db.Households
            .AsNoTracking()
            .Where(h => householdIds.Contains(h.Id) && h.IsActive)
            .ToListAsync(ct);

        var memberCounts = await db.Memberships
            .AsNoTracking()
            .Where(m => householdIds.Contains(m.HouseholdId) && m.IsActive)
            .GroupBy(m => m.HouseholdId)
            .Select(g => new { HouseholdId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var countMap = memberCounts.ToDictionary(x => x.HouseholdId, x => x.Count);

        return memberships
            .Join(households, m => m.HouseholdId, h => h.Id,
                (m, h) => new HouseholdSummaryDto(
                    h.Id.Value, h.Name, h.Description, h.CurrencyCode, h.Timezone, m.Role, m.JoinedAt,
                    countMap.GetValueOrDefault(h.Id, 0),
                    h.CreatedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<MemberDto>> ListMembersAsync(Guid householdId, CancellationToken ct = default)
    {
        var hid = HouseholdId.Create(householdId);
        // Include pending invitations (IsActive=false + InvitationCode set) alongside active members,
        // so the frontend can display "pending: code XYZ" rows next to active members.
        var memberships = await db.Memberships
            .AsNoTracking()
            .Where(m => m.HouseholdId == hid && (m.IsActive || m.InvitationCode != null))
            .ToListAsync(ct);

        var userIds = memberships.Where(m => m.IsActive).Select(m => m.UserId.Value).ToList();
        var users = await db.UserProjections
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(ct);
        var userMap = users.ToDictionary(u => u.Id);

        return memberships
            .Select(m =>
            {
                userMap.TryGetValue(m.UserId.Value, out var user);
                return new MemberDto(
                    MembershipId: m.Id.Value,
                    UserId: m.UserId.Value,
                    Username: user?.Username ?? string.Empty,
                    DisplayName: user?.DisplayName,
                    Role: m.Role,
                    JoinedAt: m.JoinedAt,
                    // Surfaces the existing domain field. Accepted memberships have IsActive=true and a cleared code.
                    PendingInvitationCode: m.IsActive ? null : m.InvitationCode);
            })
            .ToList();
    }
}
