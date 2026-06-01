using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants.Events;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants.Rules;
using LexiLink.Modules.Ads.Tests.SeedWork;

namespace LexiLink.Modules.Ads.Tests.RewardedAdGrants;

[TestFixture]
public class RewardedAdGrantTests : TestBase
{
    [Test]
    public void Create_Records_Grant_And_Publishes_Event()
    {
        var playerId = Guid.NewGuid();
        var grantedOn = DateTime.UtcNow;

        var grant = RewardedAdGrant.Create(playerId, 5, "tx-123", grantedOn);

        grant.PlayerId.Should().Be(playerId);
        grant.DiamondAmount.Should().Be(5);
        grant.TransactionId.Should().Be("tx-123");
        grant.GrantedOn.Should().Be(grantedOn);

        var published = AssertPublishedDomainEvent<RewardedAdGrantedDomainEvent>(grant);
        published.PlayerId.Should().Be(playerId);
        published.DiamondAmount.Should().Be(5);
        published.TransactionId.Should().Be("tx-123");
    }

    [Test]
    public void Create_Rejects_NonPositive_Amount()
    {
        AssertBrokenRule<RewardedAdAmountMustBePositiveRule>(
            () => RewardedAdGrant.Create(Guid.NewGuid(), 0, "tx", DateTime.UtcNow));
    }

    [Test]
    public void Create_Rejects_Empty_TransactionId()
    {
        AssertBrokenRule<TransactionIdMustNotBeEmptyRule>(
            () => RewardedAdGrant.Create(Guid.NewGuid(), 5, "   ", DateTime.UtcNow));
    }

    [Test]
    public void DailyLimitRule_Is_Broken_At_Or_Above_Limit()
    {
        new RewardedAdDailyLimitRule(9, 10).IsBroken().Should().BeFalse();
        new RewardedAdDailyLimitRule(10, 10).IsBroken().Should().BeTrue();
        new RewardedAdDailyLimitRule(11, 10).IsBroken().Should().BeTrue();
    }
}
