using Autofac;
using LexiLink.Common.Infrastructure;
using LexiLink.Common.Infrastructure.DomainEventsDispatching;
using LexiLink.Modules.Administration.Infrastructure.Configuration.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Administration.Infrastructure.Configuration;

public static class AdministrationStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap =
        LexiLink.Common.Infrastructure.DomainEventsDispatching.DomainNotificationsMap.Instance;

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AdministrationContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new AdministrationAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
