using LexiLink.Modules.Diamond.Domain.PlayerDiamondInventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Diamond.Infrastructure.Domain.PlayerDiamondInventories;

internal class PlayerDiamondInventoryEntityTypeConfiguration : IEntityTypeConfiguration<PlayerDiamondInventory>
{
    public void Configure(EntityTypeBuilder<PlayerDiamondInventory> builder)
    {
        builder.ToTable("PlayerDiamondInventories", "diamond");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PlayerId");

        builder.Property<int>("_balance").HasColumnName("Balance");
    }
}
