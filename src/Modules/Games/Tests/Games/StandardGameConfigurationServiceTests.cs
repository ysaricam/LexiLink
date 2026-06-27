using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Domain.Services;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class StandardGameConfigurationServiceTests
{
    [TestCase(3, 5)]
    [TestCase(4, 7)]
    [TestCase(5, 8)]
    public void ResolveMaxSteps_ForEasyDifficulty_UsesConfiguredDepthBudgets(
        int targetDepth,
        int expectedMaxSteps)
    {
        var service = new StandardGameConfigurationService();

        var maxSteps = service.ResolveMaxSteps(Difficulty.Easy, targetDepth);

        maxSteps.Should().Be(expectedMaxSteps);
    }
}
