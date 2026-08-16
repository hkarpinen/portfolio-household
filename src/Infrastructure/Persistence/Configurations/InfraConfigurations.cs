using Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

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
