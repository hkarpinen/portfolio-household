using Household.Application.Repositories;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using HouseholdAggregate = Household.Domain.Aggregates.Household;

namespace Infrastructure.Repositories;

internal sealed class HouseholdRepository(HouseholdDbContext db) : IHouseholdRepository
{
    public Task<HouseholdAggregate?> GetByIdAsync(HouseholdId id, CancellationToken ct) =>
        db.Households.FirstOrDefaultAsync(h => h.Id == id && h.IsActive, ct);

    public async Task AddAsync(HouseholdAggregate household, CancellationToken ct)
    {
        await db.Households.AddAsync(household, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
