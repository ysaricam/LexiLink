using LexiLink.Modules.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Payments.Infrastructure.Domain.IapPurchases;

internal class IapPurchaseEntityTypeConfiguration : IEntityTypeConfiguration<IapPurchase>
{
    public void Configure(EntityTypeBuilder<IapPurchase> builder)
    {
        builder.ToTable("IapPurchases", "payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property<Guid>("_playerId").HasColumnName("PlayerId");
        builder.Property<PaymentPlatform>("_platform").HasColumnName("Platform");
        builder.Property<PaymentEnvironment>("_environment").HasColumnName("Environment");
        builder.Property<string?>("_orderId").HasColumnName("OrderId");
        builder.Property<string?>("_clientRequestId").HasColumnName("ClientRequestId");
        builder.Property<int>("_diamondAmount").HasColumnName("DiamondAmount");
        builder.Property<IapPurchaseStatus>("_status").HasColumnName("Status");
        builder.Property<IapPurchasePostProcessingAction>("_postProcessingAction").HasColumnName("PostProcessingAction");
        builder.Property<IapPurchasePostProcessingStatus>("_postProcessingStatus").HasColumnName("PostProcessingStatus");
        builder.Property<DateTime>("_receivedAt").HasColumnName("ReceivedAt");
        builder.Property<DateTime?>("_verifiedAt").HasColumnName("VerifiedAt");
        builder.Property<DateTime?>("_grantedAt").HasColumnName("GrantedAt");
        builder.Property<string?>("_failureReason").HasColumnName("FailureReason");
        builder.Property<DateTime?>("_postProcessedAt").HasColumnName("PostProcessedAt");
        builder.Property<string?>("_postProcessingFailureReason").HasColumnName("PostProcessingFailureReason");

        builder.OwnsOne<StoreProductId>("_storeProductId", owned =>
        {
            owned.Property(x => x.Value).HasColumnName("StoreProductId");
        });

        builder.OwnsOne<StoreTransactionId>("_storeTransactionId", owned =>
        {
            owned.Property(x => x.Value).HasColumnName("StoreTransactionId");
        });

        builder.OwnsOne<PurchaseToken>("_purchaseToken", owned =>
        {
            owned.Property(x => x.Value).HasColumnName("PurchaseToken");
        });

        builder.HasIndex("_playerId");
        builder.HasIndex("_platform", "_clientRequestId");
    }
}
