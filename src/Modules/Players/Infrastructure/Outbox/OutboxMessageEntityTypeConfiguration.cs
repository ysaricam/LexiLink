using LexiLink.Common.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Players.Infrastructure.Outbox;

internal class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "players");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.OccurredOn).HasColumnName("OccurredOn");
        builder.Property(x => x.Type).HasColumnName("Type");
        builder.Property(x => x.Data).HasColumnName("Data");
        builder.Property(x => x.ProcessedDate).HasColumnName("ProcessedDate");
    }
}
