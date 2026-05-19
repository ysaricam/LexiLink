using LexiLink.Modules.Administration.Domain.AdminUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiLink.Modules.Administration.Infrastructure.Domain.AdminUsers;

internal class AdminUserEntityTypeConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUsers", "administration");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id");

        builder.OwnsOne<Email>("_email", e =>
        {
            e.Property(x => x.Value).HasColumnName("Email").IsRequired();
            e.HasIndex(x => x.Value).IsUnique();
        });
        builder.Navigation("_email").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne<AdminRole>("_role", r =>
        {
            r.Property(x => x.Value).HasColumnName("Role").IsRequired().HasMaxLength(32);
        });
        builder.Navigation("_role").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<AdminUserStatus>("_status")
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property<DateTime>("_registeredOn").HasColumnName("RegisteredOn");
        builder.Property<DateTime?>("_disabledOn").HasColumnName("DisabledOn");
    }
}
