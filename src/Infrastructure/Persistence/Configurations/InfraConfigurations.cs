using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.EventType).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
    }
}

internal sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.HasKey(e => e.EventId);
        builder.Property(e => e.EventType).HasMaxLength(200).IsRequired();
    }
}

internal sealed class UserProjectionConfiguration : IEntityTypeConfiguration<UserProjection>
{
    public void Configure(EntityTypeBuilder<UserProjection> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(100);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.IsDemo).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.DemoSeedCompletedAt);
    }
}
