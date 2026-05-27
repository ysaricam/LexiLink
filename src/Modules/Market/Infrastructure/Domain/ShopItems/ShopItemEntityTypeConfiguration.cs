using LexiLink.Modules.Market.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Market.Infrastructure.Domain.ShopItems;

internal class ShopItemEntityTypeConfiguration : IEntityTypeConfiguration<ShopItem>
{
    public void Configure(EntityTypeBuilder<ShopItem> builder)
    {
        builder.ToTable("ShopItems", "market");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property<CategoryId>("_categoryId").HasColumnName("CategoryId").IsRequired();
        builder.Property<ItemType>("_itemType").HasColumnName("ItemType");
        builder.Property<int>("_quantity").HasColumnName("Quantity");
        builder.Property<int>("_price").HasColumnName("Price");
        builder.Property<int?>("_maxStock").HasColumnName("MaxStock");
        builder.Property<int>("_soldCount").HasColumnName("SoldCount");
        builder.Property<int?>("_perPlayerLimit").HasColumnName("PerPlayerLimit");
        builder.Property<PerPlayerLimitWindow>("_perPlayerLimitWindow").HasColumnName("PerPlayerLimitWindow");
        builder.Property<bool>("_isActive").HasColumnName("IsActive");
        builder.Property(x => x.Version)
            .HasColumnName("Version")
            .IsConcurrencyToken();

        builder.OwnsOne<Promotion>("_promotion", owned =>
        {
            owned.Property(x => x.PromoPrice).HasColumnName("PromoPrice");
            owned.Property(x => x.StartsAt).HasColumnName("PromotionStartsAt");
            owned.Property(x => x.EndsAt).HasColumnName("PromotionEndsAt");
        });

        builder.HasIndex("_categoryId");
    }
}
