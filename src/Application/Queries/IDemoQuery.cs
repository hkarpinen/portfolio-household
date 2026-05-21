namespace Household.Application.Queries;

public interface IDemoQuery
{
    Task<bool> IsDemoReadyAsync(Guid userId, CancellationToken ct = default);
}
