using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Domain;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LexiLink.Modules.Ads.Infrastructure;

public class AdsContext : DbContext
{
    public DbSet<RewardedAdGrant> RewardedAdGrants { get; set; } = null!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    private readonly ILoggerFactory _loggerFactory;

    public AdsContext(DbContextOptions<AdsContext> options, ILoggerFactory loggerFactory)
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

        var typedIdTypes = typeof(AdsContext).Assembly.GetReferencedAssemblies()
            .Select(System.Reflection.Assembly.Load)
            .Concat([typeof(AdsContext).Assembly])
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
