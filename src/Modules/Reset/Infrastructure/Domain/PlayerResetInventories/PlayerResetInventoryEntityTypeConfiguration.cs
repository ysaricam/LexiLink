using LexiLink.Modules.Reset.Domain.PlayerResetInventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Reset.Infrastructure.Domain.PlayerResetInventories;

internal class PlayerResetInventoryEntityTypeConfiguration : IEntityTypeConfiguration<PlayerResetInventory>
{
    public void Configure(EntityTypeBuilder<PlayerResetInventory> builder)
    {
        builder.ToTable("PlayerResetInventories", "reset");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PlayerId");

        builder.Property<int>("_balance").HasColumnName("Balance");
    }
}
