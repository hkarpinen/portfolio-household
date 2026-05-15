using Household.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HouseholdAggregate = Household.Domain.Aggregates.Household;

namespace Infrastructure.Persistence.Configurations;

internal sealed class HouseholdConfiguration : IEntityTypeConfiguration<HouseholdAggregate>
{
    public void Configure(EntityTypeBuilder<HouseholdAggregate> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasConversion(id => id.Value, v => HouseholdId.Create(v));
        builder.Property(h => h.OwnerId)
            .HasConversion(id => id.Value, v => UserId.Create(v));
        builder.Property(h => h.Name).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Description).HasMaxLength(500);
        builder.Property(h => h.CurrencyCode).HasMaxLength(3).IsRequired();
    }
}
