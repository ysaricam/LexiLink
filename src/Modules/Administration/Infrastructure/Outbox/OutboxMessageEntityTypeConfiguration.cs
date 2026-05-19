using LexiLink.Common.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Administration.Infrastructure.Outbox;

internal class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "administration");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.OccurredOn).HasColumnName("OccurredOn");
        builder.Property(x => x.Type).HasColumnName("Type");
        builder.Property(x => x.Data).HasColumnName("Data");
        builder.Property(x => x.ProcessedDate).HasColumnName("ProcessedDate");
        builder.Property(x => x.RetryCount).HasColumnName("RetryCount");
        builder.Property(x => x.NextRetryDate).HasColumnName("NextRetryDate");
        builder.Property(x => x.Error).HasColumnName("Error");
    }
}
