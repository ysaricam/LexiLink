using System.Reflection;
using NetArchTest.Rules;

namespace LexiLink.ArchitectureTests;

[Category("ArchTests")]
public abstract class ArchitectureTestBase
{
    protected static readonly Assembly CommonDomainAssembly = typeof(Common.Domain.Entity).Assembly;
    protected static readonly Assembly CommonApplicationAssembly = typeof(Common.Application.Exceptions.NotFoundException).Assembly;
    protected static readonly Assembly CommonInfrastructureAssembly = typeof(Common.Infrastructure.IUnitOfWork).Assembly;

    protected static readonly Assembly ApiAssembly = typeof(API.Modules.Games.CategoryEndpoints).Assembly;

    protected static readonly Assembly GamesDomainAssembly = typeof(Modules.Games.Domain.Categories.Category).Assembly;
    protected static readonly Assembly GamesIntegrationEventsAssembly = typeof(Modules.Games.IntegrationEvents.GameCompletedIntegrationEvent).Assembly;
    protected static readonly Assembly GamesApplicationAssembly = typeof(Modules.Games.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly GamesInfrastructureAssembly = typeof(Modules.Games.Infrastructure.GamesContext).Assembly;

    protected static readonly Assembly PlayersDomainAssembly = typeof(Modules.Players.Domain.Players.Player).Assembly;
    protected static readonly Assembly PlayersIntegrationEventsAssembly = typeof(Modules.Players.IntegrationEvents.PlayerRegisteredIntegrationEvent).Assembly;
    protected static readonly Assembly PlayersApplicationAssembly = typeof(Modules.Players.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly PlayersInfrastructureAssembly = typeof(Modules.Players.Infrastructure.PlayersContext).Assembly;

    protected static readonly Assembly StatsApplicationAssembly = typeof(Modules.Stats.Application.Contracts.IStatsModule).Assembly;
    protected static readonly Assembly StatsInfrastructureAssembly = typeof(Modules.Stats.Infrastructure.Configuration.StatsStartup).Assembly;

    protected static readonly Assembly EnergyDomainAssembly = typeof(Modules.Energy.Domain.PlayerEnergies.PlayerEnergy).Assembly;
    protected static readonly Assembly EnergyApplicationAssembly = typeof(Modules.Energy.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly EnergyInfrastructureAssembly = typeof(Modules.Energy.Infrastructure.EnergyContext).Assembly;

    protected static readonly Assembly QuestsDomainAssembly = typeof(Modules.Quests.Domain.PlayerQuests.PlayerQuest).Assembly;
    protected static readonly Assembly QuestsIntegrationEventsAssembly = typeof(Modules.Quests.IntegrationEvents.QuestClaimedIntegrationEvent).Assembly;
    protected static readonly Assembly QuestsApplicationAssembly = typeof(Modules.Quests.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly QuestsInfrastructureAssembly = typeof(Modules.Quests.Infrastructure.QuestsContext).Assembly;

    protected static readonly Assembly AdministrationDomainAssembly = typeof(Modules.Administration.Domain.AdminUsers.AdminUser).Assembly;
    protected static readonly Assembly AdministrationIntegrationEventsAssembly = typeof(Modules.Administration.IntegrationEvents.AdminUserRegisteredIntegrationEvent).Assembly;
    protected static readonly Assembly AdministrationApplicationAssembly = typeof(Modules.Administration.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly AdministrationInfrastructureAssembly = typeof(Modules.Administration.Infrastructure.AdministrationContext).Assembly;

    protected static readonly Assembly[] ModuleAssemblies =
    [
        GamesDomainAssembly,
        GamesApplicationAssembly,
        GamesInfrastructureAssembly,
        PlayersDomainAssembly,
        PlayersApplicationAssembly,
        PlayersInfrastructureAssembly,
        StatsApplicationAssembly,
        StatsInfrastructureAssembly,
        EnergyDomainAssembly,
        EnergyApplicationAssembly,
        EnergyInfrastructureAssembly,
        QuestsDomainAssembly,
        QuestsApplicationAssembly,
        QuestsInfrastructureAssembly,
        AdministrationDomainAssembly,
        AdministrationApplicationAssembly,
        AdministrationInfrastructureAssembly
    ];

    protected static void AssertArchTestResult(TestResult result)
    {
        result.IsSuccessful.Should().BeTrue(
            "failing types: {0}",
            result.FailingTypeNames is null ? "<none>" : string.Join(", ", result.FailingTypeNames));
    }

    protected static IEnumerable<Type> GetTypes(Assembly assembly) =>
        assembly.GetTypes().Where(type => type is { IsClass: true, IsAbstract: false });
}
