using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Modules.Stats.Infrastructure.Configuration;

public static class StatsStartup
{
    public static void Initialize(IServiceCollection services, string connectionString)
    {
    }

    public static void InitializeCompositionRoot(
        ContainerBuilder containerBuilder,
        string connectionString)
    {
        containerBuilder.RegisterModule(new StatsAutofacModule(connectionString));
    }
}
