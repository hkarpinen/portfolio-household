using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class HouseholdCalendarEventConfiguration : IEntityTypeConfiguration<HouseholdCalendarEvent>
{
    public void Configure(EntityTypeBuilder<HouseholdCalendarEvent> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => CalendarEventId.Create(v));
        builder.Property(e => e.HouseholdId)
            .HasConversion(id => id.Value, v => HouseholdId.Create(v));
        builder.Property(e => e.CreatedByUserId)
            .HasConversion(id => id.Value, v => UserId.Create(v));
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.HasIndex(e => e.HouseholdId);
    }
}
