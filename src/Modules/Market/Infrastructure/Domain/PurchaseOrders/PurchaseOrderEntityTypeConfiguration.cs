using LexiLink.Modules.Market.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Market.Infrastructure.Domain.PurchaseOrders;

internal class PurchaseOrderEntityTypeConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders", "market");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property<Guid>("_playerId").HasColumnName("PlayerId");
        builder.Property<ShopItemId>("_shopItemId").HasColumnName("ShopItemId").IsRequired();
        builder.Property<ItemType>("_itemType").HasColumnName("ItemType");
        builder.Property<int>("_quantity").HasColumnName("Quantity");
        builder.Property<int>("_diamondsPaid").HasColumnName("DiamondsPaid");
        builder.Property<DateTime>("_purchasedAt").HasColumnName("PurchasedAt");
        builder.Property<string>("_idempotencyKey").HasColumnName("IdempotencyKey").HasMaxLength(128).IsRequired();

        builder.HasIndex("_shopItemId");
        builder.HasIndex("_playerId", "_idempotencyKey").IsUnique();
    }
}
