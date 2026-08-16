using Household.Domain;
using Infrastructure.Persistence.Outbox;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging;

/// <summary>
/// Publishes what the aggregates raised, and projects the same events into the activity feed, in
/// the transaction that saves them.
///
/// An interceptor rather than a <c>SaveChangesAsync</c> override: saving is what a DbContext does,
/// and a subclass that quietly does something else as well is a trap for whoever calls it.
///
/// MassTransit's bus outbox turns each <c>Publish</c> into a row in ITS outbox table on this same
/// context, so events commit with the aggregate and its delivery service sends them — which is why
/// there is no polling loop here to own.
/// </summary>
internal sealed class DomainEventPublishingInterceptor : SaveChangesInterceptor
{
    // Resolved when saving, not when constructed. Under UseBusOutbox the publish endpoint reaches
    // back for this same DbContext, so a constructor argument makes building the context require
    // building the endpoint require building the context — which hangs rather than throwing.
    private readonly IServiceProvider _services;

    public DomainEventPublishingInterceptor(IServiceProvider services) => _services = services;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is HouseholdDbContext context)
            await DrainAsync(context, cancellationToken);

        return result;
    }

    private async Task DrainAsync(HouseholdDbContext context, CancellationToken cancellationToken)
    {
        var aggregates = context.ChangeTracker.Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        if (aggregates.Count == 0) return;

        // Names come from UserProjection rows already loaded in this context; a miss falls back to
        // null rather than issuing a query from inside SaveChanges.
        var displayNames = context.ChangeTracker.Entries<UserProjection>()
            .ToDictionary(e => e.Entity.Id, e => e.Entity.DisplayName);

        string? ResolveDisplayName(Guid userId) =>
            displayNames.TryGetValue(userId, out var name) ? name : null;

        var publishEndpoint = _services.GetRequiredService<IPublishEndpoint>();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                await publishEndpoint.Publish(domainEvent, domainEvent.GetType(), cancellationToken);

                var activity = ActivityFeedProjector.TryProject(domainEvent, ResolveDisplayName);
                if (activity is not null)
                    context.ActivityEvents.Add(activity);
            }

            aggregate.ClearDomainEvents();
        }
    }
}
