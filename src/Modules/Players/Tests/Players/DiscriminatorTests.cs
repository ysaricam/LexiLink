using LexiLink.Modules.Players.Domain.Players;
using LexiLink.Modules.Players.Domain.Players.Rules;
using LexiLink.Modules.Players.Tests.SeedWork;

namespace LexiLink.Modules.Players.Tests.Players;

[TestFixture]
public class DiscriminatorTests : TestBase
{
    [Test]
    public void Of_AtMinValue_IsSuccessful()
    {
        var discriminator = Discriminator.Of(Discriminator.MinValue);
        discriminator.Value.Should().Be(Discriminator.MinValue);
    }

    [Test]
    public void Of_AtMaxValue_IsSuccessful()
    {
        var discriminator = Discriminator.Of(Discriminator.MaxValue);
        discriminator.Value.Should().Be(Discriminator.MaxValue);
    }

    [Test]
    public void Of_BelowMin_BreaksDiscriminatorMustBeInRangeRule()
    {
        AssertBrokenRule<DiscriminatorMustBeInRangeRule>(() => Discriminator.Of(0));
    }

    [Test]
    public void Of_AboveMax_BreaksDiscriminatorMustBeInRangeRule()
    {
        AssertBrokenRule<DiscriminatorMustBeInRangeRule>(() => Discriminator.Of(Discriminator.MaxValue + 1));
    }

    [Test]
    public void ToString_FormatsAsFourDigits()
    {
        Discriminator.Of(1).ToString().Should().Be("0001");
        Discriminator.Of(42).ToString().Should().Be("0042");
        Discriminator.Of(9999).ToString().Should().Be("9999");
    }

    [Test]
    public void TwoDiscriminatorsWithSameValue_AreEqual()
    {
        var a = Discriminator.Of(1234);
        var b = Discriminator.Of(1234);
        a.Should().Be(b);
    }
}
