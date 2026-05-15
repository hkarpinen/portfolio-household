using Household.Application.Repositories;
using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class ChoreRepository(HouseholdDbContext db) : IChoreRepository
{
    public Task<Chore?> GetByIdAsync(ChoreId id, CancellationToken ct) =>
        db.Chores.FirstOrDefaultAsync(c => c.Id == id && c.IsActive, ct);

    public async Task AddAsync(Chore chore, CancellationToken ct) =>
        await db.Chores.AddAsync(chore, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
