using LexiLink.Modules.Games.Domain.Games.Allowances;
using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games.Allowances;

[TestFixture]
public class ResetAllowanceTests : TestBase
{
    [Test]
    public void Of_StartsWithRemainingEqualToTotal_AndUsedZero()
    {
        var allowance = ResetAllowance.Of(2);

        allowance.Remaining.Should().Be(2);
        allowance.Used.Should().Be(0);
    }

    [Test]
    public void Consume_DecrementsRemainingAndIncrementsUsed()
    {
        var allowance = ResetAllowance.Of(2);

        var next = allowance.Consume();

        next.Remaining.Should().Be(1);
        next.Used.Should().Be(1);
    }

    [Test]
    public void Consume_WhenRemainingIsZero_BreaksResetAllowanceMustHaveRemainingRule()
    {
        var allowance = ResetAllowance.Of(0);

        AssertBrokenRule<ResetAllowanceMustHaveRemainingRule>(() => allowance.Consume());
    }
}
