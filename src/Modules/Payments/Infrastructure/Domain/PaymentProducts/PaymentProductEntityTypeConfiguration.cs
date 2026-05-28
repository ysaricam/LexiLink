using LexiLink.Modules.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Payments.Infrastructure.Domain.PaymentProducts;

internal class PaymentProductEntityTypeConfiguration : IEntityTypeConfiguration<PaymentProduct>
{
    public void Configure(EntityTypeBuilder<PaymentProduct> builder)
    {
        builder.ToTable("PaymentProducts", "payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property<int>("_diamondAmount").HasColumnName("DiamondAmount");
        builder.Property<bool>("_isAppleAvailable").HasColumnName("IsAppleAvailable");
        builder.Property<bool>("_isGoogleAvailable").HasColumnName("IsGoogleAvailable");
        builder.Property<int>("_sortOrder").HasColumnName("SortOrder");
        builder.Property<bool>("_isActive").HasColumnName("IsActive");

        builder.OwnsOne<StoreProductId>("_storeProductId", owned =>
        {
            owned.Property(x => x.Value).HasColumnName("StoreProductId");
            owned.HasIndex(x => x.Value).IsUnique();
        });
    }
}
