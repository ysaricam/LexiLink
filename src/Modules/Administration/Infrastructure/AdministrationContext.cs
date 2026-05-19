using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Domain;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Administration.Domain.AdminUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LexiLink.Modules.Administration.Infrastructure;

public class AdministrationContext : DbContext
{
    public DbSet<AdminUser> AdminUsers { get; set; } = null!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    private readonly ILoggerFactory _loggerFactory;

    public AdministrationContext(DbContextOptions<AdministrationContext> options, ILoggerFactory loggerFactory)
        : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseLoggerFactory(_loggerFactory).EnableSensitiveDataLogging();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        var typedIdTypes = typeof(AdministrationContext).Assembly.GetReferencedAssemblies()
            .Select(System.Reflection.Assembly.Load)
            .Concat([typeof(AdministrationContext).Assembly])
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false }
                        && typeof(TypedIdValueBase).IsAssignableFrom(t));

        foreach (var typedIdType in typedIdTypes)
        {
            var converterType = typeof(TypedIdValueConverter<>).MakeGenericType(typedIdType);
            configurationBuilder.Properties(typedIdType).HaveConversion(converterType);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
}
