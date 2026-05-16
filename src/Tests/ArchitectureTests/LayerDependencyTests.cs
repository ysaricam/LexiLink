using NetArchTest.Rules;

namespace LexiLink.ArchitectureTests;

[TestFixture]
public class LayerDependencyTests : ArchitectureTestBase
{
    [Test]
    public void ApiModuleEndpoints_Should_NotDependOnMediatR_OrModuleInfrastructure()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("LexiLink.API.Modules")
            .Should()
            .NotHaveDependencyOnAny(
                "MediatR",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats.Infrastructure",
                "LexiLink.Modules.Energy.Infrastructure",
                "LexiLink.Modules.Quests.Infrastructure")
            .GetResult();

        AssertArchTestResult(result);
    }

    [Test]
    public void ApiCompositionRoot_Should_NotDependOnModuleWiringDetails()
    {
        var result = Types.InAssembly(ApiAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "LexiLink.Modules.Games.Infrastructure.Configuration.Outbox",
                "LexiLink.Modules.Players.Infrastructure.Configuration.Outbox",
                "LexiLink.Modules.Energy.Infrastructure.Configuration.Outbox",
                "LexiLink.Modules.Quests.Infrastructure.Configuration.Outbox",
                "LexiLink.Modules.Games.Infrastructure.GamesContext",
                "LexiLink.Modules.Players.Infrastructure.PlayersContext",
                "LexiLink.Modules.Energy.Infrastructure.EnergyContext",
                "LexiLink.Modules.Quests.Infrastructure.QuestsContext",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertArchTestResult(result);
    }

    [TestCaseSource(nameof(ModuleLayerRules))]
    public void ModuleLayer_Should_NotDependOnForbiddenNamespaces(
        string ruleName,
        System.Reflection.Assembly assembly,
        string[] forbiddenNamespaces)
    {
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOnAny(forbiddenNamespaces)
            .GetResult();

        AssertArchTestResult(result);
    }

    private static IEnumerable<TestCaseData> ModuleLayerRules()
    {
        yield return new TestCaseData(
            "Games.Domain",
            GamesDomainAssembly,
            new[]
            {
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Players.Domain",
            PlayersDomainAssembly,
            new[]
            {
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Games.Application",
            GamesApplicationAssembly,
            new[]
            {
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy.Domain",
                "LexiLink.Modules.Energy.Application",
                "LexiLink.Modules.Energy.Infrastructure",
                "LexiLink.Modules.Quests",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Players.Application",
            PlayersApplicationAssembly,
            new[]
            {
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Games.Infrastructure",
            GamesInfrastructureAssembly,
            new[]
            {
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests",
                "LexiLink.API"
            });

        yield return new TestCaseData(
            "Players.Infrastructure",
            PlayersInfrastructureAssembly,
            new[]
            {
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests",
                "LexiLink.API"
            });

        yield return new TestCaseData(
            "Stats.Application",
            StatsApplicationAssembly,
            new[]
            {
                "LexiLink.Modules.Stats.Infrastructure",
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Stats.Infrastructure",
            StatsInfrastructureAssembly,
            new[]
            {
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests",
                "LexiLink.API"
            });

        yield return new TestCaseData(
            "Energy.Domain",
            EnergyDomainAssembly,
            new[]
            {
                "LexiLink.Modules.Energy.Application",
                "LexiLink.Modules.Energy.Infrastructure",
                "LexiLink.Modules.Games",
                "LexiLink.Modules.Players",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Quests",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "Npgsql"
            });

        // Energy.Application MAY reference Quests.IntegrationEvents (public
        // contract assembly) — granular allow analogous to how Energy already
        // consumes Players.IntegrationEvents. Quests.Domain/Application/
        // Infrastructure remain forbidden.
        yield return new TestCaseData(
            "Energy.Application",
            EnergyApplicationAssembly,
            new[]
            {
                "LexiLink.Modules.Energy.Infrastructure",
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Quests.Domain",
                "LexiLink.Modules.Quests.Application",
                "LexiLink.Modules.Quests.Infrastructure",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Energy.Infrastructure",
            EnergyInfrastructureAssembly,
            new[]
            {
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Quests",
                "LexiLink.API"
            });

        yield return new TestCaseData(
            "Quests.Domain",
            QuestsDomainAssembly,
            new[]
            {
                "LexiLink.Modules.Quests.Application",
                "LexiLink.Modules.Quests.Infrastructure",
                "LexiLink.Modules.Games",
                "LexiLink.Modules.Players",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "Npgsql"
            });

        // Quests.Application is allowed to reference Players.IntegrationEvents and
        // Games.IntegrationEvents (public contract assemblies) — these are
        // analogous to how Stats / Energy consume integration events.
        yield return new TestCaseData(
            "Quests.Application",
            QuestsApplicationAssembly,
            new[]
            {
                "LexiLink.Modules.Quests.Infrastructure",
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Quests.Infrastructure",
            QuestsInfrastructureAssembly,
            new[]
            {
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.API"
            });
    }

    [Test]
    public void IntegrationEvents_Should_NotDependOnModuleInternals()
    {
        var result = Types.InAssemblies([
                GamesIntegrationEventsAssembly,
                PlayersIntegrationEventsAssembly,
                QuestsIntegrationEventsAssembly
            ])
            .Should()
            .NotHaveDependencyOnAny(
                "LexiLink.Modules.Games.Domain",
                "LexiLink.Modules.Games.Application",
                "LexiLink.Modules.Games.Infrastructure",
                "LexiLink.Modules.Players.Domain",
                "LexiLink.Modules.Players.Application",
                "LexiLink.Modules.Players.Infrastructure",
                "LexiLink.Modules.Stats",
                "LexiLink.Modules.Energy",
                "LexiLink.Modules.Quests.Domain",
                "LexiLink.Modules.Quests.Application",
                "LexiLink.Modules.Quests.Infrastructure",
                "LexiLink.API",
                "MediatR",
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "Npgsql")
            .GetResult();

        AssertArchTestResult(result);
    }
}
