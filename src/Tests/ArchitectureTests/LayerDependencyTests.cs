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
                "LexiLink.Modules.Players.Infrastructure")
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
                "LexiLink.Modules.Games.Infrastructure.GamesContext",
                "LexiLink.Modules.Players.Infrastructure.PlayersContext",
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
                "LexiLink.Modules.Players",
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
                "LexiLink.Modules.Games",
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
                "LexiLink.Modules.Players",
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
                "LexiLink.Modules.Games",
                "LexiLink.API",
                "Microsoft.EntityFrameworkCore",
                "Npgsql"
            });

        yield return new TestCaseData(
            "Games.Infrastructure",
            GamesInfrastructureAssembly,
            new[]
            {
                "LexiLink.Modules.Players",
                "LexiLink.API"
            });

        yield return new TestCaseData(
            "Players.Infrastructure",
            PlayersInfrastructureAssembly,
            new[]
            {
                "LexiLink.Modules.Games",
                "LexiLink.API"
            });
    }
}
