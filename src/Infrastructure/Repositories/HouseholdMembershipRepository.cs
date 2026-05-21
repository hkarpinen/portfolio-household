using Household.Application.Repositories;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class HouseholdMembershipRepository(HouseholdDbContext db) : IHouseholdMembershipRepository
{
    public Task<HouseholdMembership?> GetByIdAsync(MembershipId id, CancellationToken ct) =>
        db.Memberships.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<HouseholdMembership?> GetByHouseholdAndUserAsync(HouseholdId householdId, UserId userId, CancellationToken ct) =>
        db.Memberships.FirstOrDefaultAsync(m => m.HouseholdId == householdId && m.UserId == userId, ct);

    public Task<HouseholdMembership?> GetByInvitationCodeAsync(string code, CancellationToken ct) =>
        db.Memberships.FirstOrDefaultAsync(m => m.InvitationCode == code && !m.IsActive, ct);

    public async Task<IReadOnlyList<HouseholdMembership>> ListByHouseholdAsync(HouseholdId householdId, CancellationToken ct) =>
        await db.Memberships.Where(m => m.HouseholdId == householdId && m.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<HouseholdMembership>> ListByUserAsync(UserId userId, CancellationToken ct) =>
        await db.Memberships.Where(m => m.UserId == userId && m.IsActive).ToListAsync(ct);

    public async Task AddAsync(HouseholdMembership membership, CancellationToken ct)
    {
        await db.Memberships.AddAsync(membership, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public Task DeleteByUserAsync(UserId userId, CancellationToken ct) =>
        db.Memberships.Where(m => m.UserId == userId).ExecuteDeleteAsync(ct);
}
