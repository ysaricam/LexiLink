using LexiLink.Modules.Energy.Domain.PlayerEnergies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Energy.Infrastructure.Domain.PlayerEnergies;

internal class PlayerEnergyEntityTypeConfiguration : IEntityTypeConfiguration<PlayerEnergy>
{
    public void Configure(EntityTypeBuilder<PlayerEnergy> builder)
    {
        builder.ToTable("PlayerEnergies", "energy");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PlayerId");

        builder.Property<int>("_currentAmount").HasColumnName("CurrentAmount");
        builder.Property<int>("_maximumAmount").HasColumnName("MaximumAmount");
        builder.Property<int>("_rechargeIntervalSeconds").HasColumnName("RechargeIntervalSeconds");
        builder.Property<DateTime>("_lastRefilledOn").HasColumnName("LastRefilledOn");
    }
}
