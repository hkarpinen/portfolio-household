using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class ChoreConfiguration : IEntityTypeConfiguration<Chore>
{
    public void Configure(EntityTypeBuilder<Chore> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, v => ChoreId.Create(v));
        builder.Property(c => c.HouseholdId)
            .HasConversion(id => id.Value, v => HouseholdId.Create(v));
        builder.Property(c => c.CreatedByUserId)
            .HasConversion(id => id.Value, v => UserId.Create(v));
        builder.Property(c => c.AssignedToUserId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                           v => v.HasValue ? UserId.Create(v.Value) : (UserId?)null);
        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(1000);
        builder.HasIndex(c => c.HouseholdId);
    }
}
