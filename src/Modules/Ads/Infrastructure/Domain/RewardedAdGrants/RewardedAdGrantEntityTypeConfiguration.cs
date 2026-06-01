using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Ads.Infrastructure.Domain.RewardedAdGrants;

internal class RewardedAdGrantEntityTypeConfiguration : IEntityTypeConfiguration<RewardedAdGrant>
{
    public void Configure(EntityTypeBuilder<RewardedAdGrant> builder)
    {
        builder.ToTable("RewardedAdGrants", "ads");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.PlayerId).HasColumnName("PlayerId");
        builder.Property(x => x.DiamondAmount).HasColumnName("DiamondAmount");
        builder.Property(x => x.TransactionId).HasColumnName("TransactionId");
        builder.Property(x => x.GrantedOn).HasColumnName("GrantedOn");

        builder.HasIndex(x => x.TransactionId).IsUnique();
    }
}
