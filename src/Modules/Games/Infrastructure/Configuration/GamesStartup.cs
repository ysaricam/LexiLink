using Autofac;
using LexiLink.Common.Application;
using LexiLink.Common.Infrastructure;
using LexiLink.Modules.Games.Infrastructure.Configuration.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Games.Infrastructure.Configuration;

public static class GamesStartup
{
    private static readonly BiDictionary<string, Type> DomainNotificationsMap = new();

    public static void Initialize(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<GamesContext>(opts =>
            opts.UseNpgsql(connectionString)
                .ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>());
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new GamesAutofacModule(connectionString));
        containerBuilder.RegisterModule(new OutboxModule(DomainNotificationsMap));
    }

    public static void CheckMappings() =>
        OutboxModule.CheckMappings(DomainNotificationsMap);
}
