using Household.Domain;
using Household.Domain.Aggregates;
using Infrastructure.Persistence.Outbox;
using MassTransit;
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
    public DbSet<ActivityEventRecord> ActivityEvents => Set<ActivityEventRecord>();

    public HouseholdDbContext(DbContextOptions<HouseholdDbContext> options) : base(options) { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("household");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HouseholdDbContext).Assembly);

        // MassTransit's transactional outbox and inbox, replacing the hand-rolled outbox_messages
        // table, its polling publisher, and the processed_events dedup.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Ignore backing domain event collection from all aggregate roots
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var prop = entityType.ClrType.GetProperty("DomainEvents");
            if (prop != null)
                modelBuilder.Entity(entityType.ClrType).Ignore("DomainEvents");
        }
    }
}
