using LexiLink.Common.Application.Outbox;
using LexiLink.Common.Domain;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Payments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LexiLink.Modules.Payments.Infrastructure;

public class PaymentsContext : DbContext
{
    public DbSet<PaymentProduct> PaymentProducts { get; set; } = null!;

    public DbSet<IapPurchase> IapPurchases { get; set; } = null!;

    public DbSet<PaymentNotification> PaymentNotifications { get; set; } = null!;

    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    private readonly ILoggerFactory _loggerFactory;

    public PaymentsContext(DbContextOptions<PaymentsContext> options, ILoggerFactory loggerFactory)
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

        var typedIdTypes = typeof(PaymentsContext).Assembly.GetReferencedAssemblies()
            .Select(System.Reflection.Assembly.Load)
            .Concat([typeof(PaymentsContext).Assembly])
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
