namespace Household.Application.Managers.Demo;

public interface IDemoSeedManager
{
    // Null if the user already has a household — seeding is idempotent.
    Task<Guid?> SeedAsync(Guid userId, string displayName, CancellationToken ct = default);
    Task CleanupAsync(Guid userId, CancellationToken ct = default);
}
