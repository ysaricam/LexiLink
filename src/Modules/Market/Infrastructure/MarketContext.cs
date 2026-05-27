using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Domain;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Market.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LexiLink.Modules.Market.Infrastructure;

public class MarketContext : DbContext
{
    public DbSet<Category> Categories { get; set; } = null!;

    public DbSet<ShopItem> ShopItems { get; set; } = null!;

    public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    private readonly ILoggerFactory _loggerFactory;

    public MarketContext(DbContextOptions<MarketContext> options, ILoggerFactory loggerFactory)
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

        var typedIdTypes = typeof(MarketContext).Assembly.GetReferencedAssemblies()
            .Select(System.Reflection.Assembly.Load)
            .Concat([typeof(MarketContext).Assembly])
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
