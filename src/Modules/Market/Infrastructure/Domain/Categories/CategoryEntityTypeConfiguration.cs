using LexiLink.Modules.Market.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Market.Infrastructure.Domain.Categories;

internal class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", "market");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property<string>("_name").HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property<int>("_sortOrder").HasColumnName("SortOrder");
        builder.Property<string?>("_icon").HasColumnName("Icon").HasMaxLength(64);
        builder.Property<bool>("_isActive").HasColumnName("IsActive");

        builder.OwnsOne<VisibilityWindow>("_visibilityWindow", owned =>
        {
            owned.Property(x => x.StartsAt).HasColumnName("VisibilityStartsAt");
            owned.Property(x => x.EndsAt).HasColumnName("VisibilityEndsAt");
        });
    }
}
