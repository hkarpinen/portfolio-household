using Household.Domain.Aggregates;
using Household.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class HouseholdMembershipConfiguration : IEntityTypeConfiguration<HouseholdMembership>
{
    public void Configure(EntityTypeBuilder<HouseholdMembership> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, v => MembershipId.Create(v));
        builder.Property(m => m.HouseholdId)
            .HasConversion(id => id.Value, v => HouseholdId.Create(v));
        builder.Property(m => m.UserId)
            .HasConversion(id => id.Value, v => UserId.Create(v));
        builder.Property(m => m.InvitationCode).HasMaxLength(8);
        builder.HasIndex(m => new { m.HouseholdId, m.UserId });
        builder.HasIndex(m => m.InvitationCode).IsUnique().HasFilter("invitation_code IS NOT NULL");
    }
}
