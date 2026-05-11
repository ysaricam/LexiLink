using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class ScoreTests : TestBase
{
    [Test]
    public void Of_StoresPoints()
    {
        var score = Score.Of(250);

        score.Points.Should().Be(250);
    }

    [Test]
    public void Of_AllowsZero()
    {
        var score = Score.Of(0);

        score.Points.Should().Be(0);
    }

    [Test]
    public void TwoScoresWithSamePoints_AreEqual()
    {
        var a = Score.Of(100);
        var b = Score.Of(100);

        a.Should().Be(b);
    }
}
