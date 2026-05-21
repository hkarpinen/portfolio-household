namespace Household.Application.Managers.Demo;

public interface IDemoSeedManager
{
    /// <summary>
    /// Seeds a demo household for the given user.
    /// Returns the new household's ID, or null if the user already has a household (idempotent).
    /// </summary>
    Task<Guid?> SeedAsync(Guid userId, string displayName, CancellationToken ct = default);
    Task CleanupAsync(Guid userId, CancellationToken ct = default);
}
