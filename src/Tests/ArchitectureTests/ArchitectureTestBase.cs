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

    protected static readonly Assembly DiamondDomainAssembly = typeof(Modules.Diamond.Domain.PlayerDiamondInventories.PlayerDiamondInventory).Assembly;
    protected static readonly Assembly DiamondApplicationAssembly = typeof(Modules.Diamond.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly DiamondInfrastructureAssembly = typeof(Modules.Diamond.Infrastructure.DiamondContext).Assembly;

    protected static readonly Assembly HintDomainAssembly = typeof(Modules.Hint.Domain.PlayerHintInventories.PlayerHintInventory).Assembly;
    protected static readonly Assembly HintApplicationAssembly = typeof(Modules.Hint.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly HintInfrastructureAssembly = typeof(Modules.Hint.Infrastructure.HintContext).Assembly;

    protected static readonly Assembly UndoDomainAssembly = typeof(Modules.Undo.Domain.PlayerUndoInventories.PlayerUndoInventory).Assembly;
    protected static readonly Assembly UndoApplicationAssembly = typeof(Modules.Undo.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly UndoInfrastructureAssembly = typeof(Modules.Undo.Infrastructure.UndoContext).Assembly;

    protected static readonly Assembly ResetDomainAssembly = typeof(Modules.Reset.Domain.PlayerResetInventories.PlayerResetInventory).Assembly;
    protected static readonly Assembly ResetApplicationAssembly = typeof(Modules.Reset.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly ResetInfrastructureAssembly = typeof(Modules.Reset.Infrastructure.ResetContext).Assembly;

    protected static readonly Assembly MarketDomainAssembly = typeof(Modules.Market.Domain.ShopItem).Assembly;
    protected static readonly Assembly MarketIntegrationEventsAssembly = typeof(Modules.Market.IntegrationEvents.PurchaseCompletedIntegrationEvent).Assembly;
    protected static readonly Assembly MarketApplicationAssembly = typeof(Modules.Market.Application.Contracts.ICommand).Assembly;
    protected static readonly Assembly MarketInfrastructureAssembly = typeof(Modules.Market.Infrastructure.MarketContext).Assembly;

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
        AdministrationInfrastructureAssembly,
        DiamondDomainAssembly,
        DiamondApplicationAssembly,
        DiamondInfrastructureAssembly,
        HintDomainAssembly,
        HintApplicationAssembly,
        HintInfrastructureAssembly,
        UndoDomainAssembly,
        UndoApplicationAssembly,
        UndoInfrastructureAssembly,
        ResetDomainAssembly,
        ResetApplicationAssembly,
        ResetInfrastructureAssembly,
        MarketDomainAssembly,
        MarketApplicationAssembly,
        MarketInfrastructureAssembly
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
