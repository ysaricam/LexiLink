using LexiLink.Modules.Players.Domain.Players;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Players.Infrastructure.Domain.Players;

internal class PlayerEntityTypeConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players", "players");

        builder.HasKey(x => x.Id);

        // Ignore the public AuthIdentities getter; the backing field _authIdentities
        // is mapped via OwnsMany below. Without this, EF auto-discovers AuthIdentities
        // as a second navigation and fails model validation.
        builder.Ignore(p => p.AuthIdentities);

        builder.Property<string>("_displayName").HasColumnName("DisplayName");
        builder.Property<string?>("_avatarUrl").HasColumnName("AvatarUrl");
        builder.Property<string>("_locale").HasColumnName("Locale");
        builder.Property<DateTime>("_createdAt").HasColumnName("CreatedAt");
        builder.Property<bool>("_isGuest").HasColumnName("IsGuest");

        builder.OwnsOne<Discriminator>("_discriminator", d =>
        {
            d.Property(x => x.Value).HasColumnName("DiscriminatorValue");
        });
        builder.Navigation("_discriminator").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<AuthIdentity>("_authIdentities", a =>
        {
            a.ToTable("PlayerAuthIdentities", "players");
            a.WithOwner().HasForeignKey("PlayerId");
            a.Property(x => x.Provider)
                .HasColumnName("Provider")
                .HasConversion<string>()
                .HasMaxLength(32);
            a.Property(x => x.ExternalId).HasColumnName("ExternalId");
            a.Property(x => x.Email).HasColumnName("Email");
            a.Property(x => x.LinkedAt).HasColumnName("LinkedAt");
            a.HasKey("PlayerId", "Provider");
        });
        builder.Navigation("_authIdentities").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
