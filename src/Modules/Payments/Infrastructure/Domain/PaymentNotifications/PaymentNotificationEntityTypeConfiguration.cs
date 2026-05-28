using LexiLink.Modules.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Payments.Infrastructure.Domain.PaymentNotifications;

internal class PaymentNotificationEntityTypeConfiguration : IEntityTypeConfiguration<PaymentNotification>
{
    public void Configure(EntityTypeBuilder<PaymentNotification> builder)
    {
        builder.ToTable("PaymentNotifications", "payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property<PaymentPlatform>("_platform").HasColumnName("Platform");
        builder.Property<PaymentEnvironment>("_environment").HasColumnName("Environment");
        builder.Property<string>("_notificationId").HasColumnName("NotificationId");
        builder.Property<string>("_notificationType").HasColumnName("NotificationType");
        builder.Property<string>("_payloadJson").HasColumnName("PayloadJson");
        builder.Property<DateTime>("_receivedAt").HasColumnName("ReceivedAt");
        builder.Property<DateTime?>("_processedAt").HasColumnName("ProcessedAt");
        builder.Property<PaymentNotificationStatus>("_status").HasColumnName("Status");
        builder.Property<string?>("_failureReason").HasColumnName("FailureReason");

        builder.HasIndex("_platform", "_notificationId").IsUnique();
    }
}
