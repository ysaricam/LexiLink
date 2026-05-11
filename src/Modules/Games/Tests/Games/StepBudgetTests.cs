using LexiLink.Modules.Games.Domain.Games;
using LexiLink.Modules.Games.Tests.SeedWork;

namespace LexiLink.Modules.Games.Tests.Games;

[TestFixture]
public class StepBudgetTests : TestBase
{
    [Test]
    public void Of_StartsWithMaxAndZeroTaken()
    {
        var budget = StepBudget.Of(5);

        budget.Max.Should().Be(5);
        budget.Taken.Should().Be(0);
        budget.Remaining.Should().Be(5);
    }

    [Test]
    public void Step_IncrementsTaken()
    {
        var budget = StepBudget.Of(3);

        var next = budget.Step();

        next.Taken.Should().Be(1);
        next.Remaining.Should().Be(2);
    }

    [Test]
    public void UndoStep_DecrementsTaken()
    {
        var budget = StepBudget.Of(3).Step().Step();

        var next = budget.UndoStep();

        next.Taken.Should().Be(1);
    }

    [Test]
    public void UndoStep_WhenTakenIsZero_StaysAtZero()
    {
        var budget = StepBudget.Of(3);

        var next = budget.UndoStep();

        next.Taken.Should().Be(0);
    }

    [Test]
    public void Reset_ReturnsTakenToZero()
    {
        var budget = StepBudget.Of(3).Step().Step();

        var next = budget.Reset();

        next.Taken.Should().Be(0);
        next.Max.Should().Be(3);
    }

    [Test]
    public void IsExhausted_WhenTakenEqualsMax_IsTrue()
    {
        var budget = StepBudget.Of(2).Step().Step();

        budget.IsExhausted.Should().BeTrue();
    }

    [Test]
    public void IsExhausted_WhenBelowMax_IsFalse()
    {
        var budget = StepBudget.Of(2).Step();

        budget.IsExhausted.Should().BeFalse();
    }

    [Test]
    public void IsAtLastWarning_WhenTakenEqualsMaxMinusOne_IsTrue()
    {
        var budget = StepBudget.Of(3).Step().Step();

        budget.IsAtLastWarning.Should().BeTrue();
    }

    [Test]
    public void IsBelowLastWarning_WhenTakenIsLessThanMaxMinusOne_IsTrue()
    {
        var budget = StepBudget.Of(3).Step();

        budget.IsBelowLastWarning.Should().BeTrue();
    }

    [Test]
    public void IsBelowLastWarning_WhenAtLastWarning_IsFalse()
    {
        var budget = StepBudget.Of(3).Step().Step();

        budget.IsBelowLastWarning.Should().BeFalse();
    }
}
