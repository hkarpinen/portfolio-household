using Household.Domain;
using Household.Domain.Aggregates;
using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using HouseholdAggregate = Household.Domain.Aggregates.Household;

namespace Infrastructure.Persistence;

public sealed class HouseholdDbContext : DbContext
{
    public DbSet<HouseholdAggregate> Households => Set<HouseholdAggregate>();
    public DbSet<HouseholdMembership> Memberships => Set<HouseholdMembership>();
    public DbSet<Chore> Chores => Set<Chore>();
    public DbSet<HouseholdCalendarEvent> CalendarEvents => Set<HouseholdCalendarEvent>();
    public DbSet<UserProjection> UserProjections => Set<UserProjection>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ActivityEventRecord> ActivityEvents => Set<ActivityEventRecord>();

    public HouseholdDbContext(DbContextOptions<HouseholdDbContext> options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DrainDomainEventsToOutbox();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void DrainDomainEventsToOutbox()
    {
        var aggregates = ChangeTracker.Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        // Build a lookup of user display names from the change-tracked UserProjection rows
        // that are already loaded in this DbContext instance. Falls back to empty string when
        // the projection row is not in memory — avoids a synchronous DB call here.
        var userDisplayNames = ChangeTracker.Entries<UserProjection>()
            .ToDictionary(e => e.Entity.Id, e => e.Entity.DisplayName);

        string? ResolveDisplayName(Guid userId)
        {
            userDisplayNames.TryGetValue(userId, out var name);
            return name;
        }

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                this.AddToOutbox(domainEvent);

                // Also project relevant events into the activity feed read model.
                var activity = ActivityFeedProjector.TryProject(domainEvent, ResolveDisplayName);
                if (activity is not null)
                    ActivityEvents.Add(activity);
            }
            aggregate.ClearDomainEvents();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("household");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HouseholdDbContext).Assembly);

        // Ignore backing domain event collection from all aggregate roots
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var prop = entityType.ClrType.GetProperty("DomainEvents");
            if (prop != null)
                modelBuilder.Entity(entityType.ClrType).Ignore("DomainEvents");
        }
    }
}
