using LexiLink.Modules.Games.Domain.Categories;
using LexiLink.Modules.Games.Domain.Links;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Links;

internal class LinkEntityTypeConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable("Links", "games");

        builder.HasKey(x => x.Id);

        builder.Property<string>("_value").HasColumnName("Value");
        builder.Property<string>("_description").HasColumnName("Description");
        builder.Property<bool>("_isActive").HasColumnName("IsActive");
        builder.Property<CategoryId>("_categoryId").HasColumnName("CategoryId");

        builder.OwnsMany<OutgoingLink>("_outgoingLinks", b =>
        {
            b.ToTable("LinkOutgoingLinks", "games");
            b.WithOwner().HasForeignKey("LinkId");
            b.Property(o => o.TargetId).HasColumnName("OutgoingLinkId");
            b.HasKey("LinkId", "TargetId");
        });

        builder.Navigation("_outgoingLinks").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
