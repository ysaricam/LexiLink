using LexiLink.Modules.Games.Domain.Games.Allowances;
using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games.Allowances;

[TestFixture]
public class HintAllowanceTests : TestBase
{
    [Test]
    public void Of_StartsWithRemainingEqualToTotal_AndUsedZero()
    {
        var allowance = HintAllowance.Of(3);

        allowance.Remaining.Should().Be(3);
        allowance.Used.Should().Be(0);
    }

    [Test]
    public void Consume_DecrementsRemainingAndIncrementsUsed()
    {
        var allowance = HintAllowance.Of(3);

        var next = allowance.Consume();

        next.Remaining.Should().Be(2);
        next.Used.Should().Be(1);
    }

    [Test]
    public void Consume_IsImmutable()
    {
        var original = HintAllowance.Of(3);

        original.Consume();

        original.Remaining.Should().Be(3);
        original.Used.Should().Be(0);
    }

    [Test]
    public void Consume_WhenRemainingIsZero_BreaksHintAllowanceMustHaveRemainingRule()
    {
        var allowance = HintAllowance.Of(0);

        AssertBrokenRule<HintAllowanceMustHaveRemainingRule>(() => allowance.Consume());
    }
}
