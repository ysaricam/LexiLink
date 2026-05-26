using LexiLink.Modules.Undo.Domain.PlayerUndoInventories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Undo.Infrastructure.Domain.PlayerUndoInventories;

internal class PlayerUndoInventoryEntityTypeConfiguration : IEntityTypeConfiguration<PlayerUndoInventory>
{
    public void Configure(EntityTypeBuilder<PlayerUndoInventory> builder)
    {
        builder.ToTable("PlayerUndoInventories", "undo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("PlayerId");

        builder.Property<int>("_balance").HasColumnName("Balance");
    }
}
