using LexiLink.Modules.Games.Domain.Games.Allowances;
using LexiLink.Modules.Games.Domain.Games.Allowances.Rules;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games.Allowances;

[TestFixture]
public class UndoAllowanceTests : TestBase
{
    [Test]
    public void Of_StartsWithRemainingEqualToTotal_AndUsedZero()
    {
        var allowance = UndoAllowance.Of(5);

        allowance.Remaining.Should().Be(5);
        allowance.Used.Should().Be(0);
    }

    [Test]
    public void Consume_DecrementsRemainingAndIncrementsUsed()
    {
        var allowance = UndoAllowance.Of(5);

        var next = allowance.Consume();

        next.Remaining.Should().Be(4);
        next.Used.Should().Be(1);
    }

    [Test]
    public void Consume_WhenRemainingIsZero_BreaksUndoAllowanceMustHaveRemainingRule()
    {
        var allowance = UndoAllowance.Of(0);

        AssertBrokenRule<UndoAllowanceMustHaveRemainingRule>(() => allowance.Consume());
    }
}
