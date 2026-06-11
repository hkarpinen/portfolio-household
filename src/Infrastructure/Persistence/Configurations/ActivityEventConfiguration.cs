using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ActivityEventConfiguration : IEntityTypeConfiguration<ActivityEventRecord>
{
    public void Configure(EntityTypeBuilder<ActivityEventRecord> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EventType).HasMaxLength(64).IsRequired();
        builder.Property(e => e.ActorDisplayName).HasMaxLength(200).IsRequired();
        builder.Property(e => e.TargetDescription).HasMaxLength(500);

        builder.HasIndex(e => new { e.HouseholdId, e.OccurredAt })
            .HasDatabaseName("ix_activity_events_household_id_occurred_at")
            .IsDescending(false, true);
    }
}
