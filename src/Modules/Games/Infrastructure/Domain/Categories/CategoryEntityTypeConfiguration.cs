using LexiLink.Modules.Games.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Games.Infrastructure.Domain.Categories;

internal class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", "games");

        builder.HasKey(x => x.Id);

        builder.Property<string>("_name").HasColumnName("Name");
        builder.Property<string>("_description").HasColumnName("Description");
        builder.Property<string>("_language").HasColumnName("Language");
    }
}
