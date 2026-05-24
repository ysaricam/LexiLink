using LexiLink.Modules.Hint.Domain.PlayerHintInventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Hint.Infrastructure.Domain.PlayerHintInventories;

internal class PlayerHintInventoryEntityTypeConfiguration : IEntityTypeConfiguration<PlayerHintInventory>
{
    public void Configure(EntityTypeBuilder<PlayerHintInventory> builder)
    {
        builder.ToTable("PlayerHintInventories", "hint");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PlayerId");

        builder.Property<int>("_balance").HasColumnName("Balance");
    }
}
