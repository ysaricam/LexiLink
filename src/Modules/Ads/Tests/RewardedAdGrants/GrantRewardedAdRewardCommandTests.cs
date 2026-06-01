using LexiLink.Common.Application.Time;
using LexiLink.Modules.Ads.Application.Configuration.Verification;
using LexiLink.Modules.Ads.Application.RewardedAdGrants.GrantRewardedAdReward;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;

namespace LexiLink.Modules.Ads.Tests.RewardedAdGrants;

[TestFixture]
public class GrantRewardedAdRewardCommandTests
{
    private static readonly DateTime Now = new(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task Grants_Diamond_And_Records_Ledger_When_Verified_And_Under_Cap()
    {
        var playerId = Guid.NewGuid();
        var repository = new FakeRewardedAdGrantRepository();
        var diamondGrant = new FakeDiamondGrant();
        var handler = CreateHandler(repository, new FakeAdMobSsvVerifier(verified: true), diamondGrant);

        var result = await handler.Handle(Command(playerId, "tx-1"), CancellationToken.None);

        result.Outcome.Should().Be(RewardedAdGrantOutcome.Granted.ToString());
        result.DiamondAmount.Should().Be(5);
        result.GrantsToday.Should().Be(1);
        result.RemainingToday.Should().Be(9);
        diamondGrant.Grants.Should().ContainSingle().Which.Should().Be((playerId, 5));
        repository.Grants.Should().ContainSingle().Which.TransactionId.Should().Be("tx-1");
    }

    [Test]
    public async Task Replayed_Transaction_Returns_AlreadyGranted_Without_Second_Grant()
    {
        var playerId = Guid.NewGuid();
        var repository = new FakeRewardedAdGrantRepository();
        var diamondGrant = new FakeDiamondGrant();
        var handler = CreateHandler(repository, new FakeAdMobSsvVerifier(verified: true), diamondGrant);

        var first = await handler.Handle(Command(playerId, "tx-1"), CancellationToken.None);
        var replay = await handler.Handle(Command(playerId, "tx-1"), CancellationToken.None);

        first.Outcome.Should().Be(RewardedAdGrantOutcome.Granted.ToString());
        replay.Outcome.Should().Be(RewardedAdGrantOutcome.AlreadyGranted.ToString());
        diamondGrant.Grants.Should().ContainSingle();
        repository.Grants.Should().ContainSingle();
    }

    [Test]
    public async Task Daily_Cap_Reached_Returns_DailyLimitReached_Without_Grant()
    {
        var playerId = Guid.NewGuid();
        var repository = new FakeRewardedAdGrantRepository();
        for (var i = 0; i < 10; i++)
        {
            repository.Seed(RewardedAdGrant.Create(playerId, 5, $"seed-{i}", Now));
        }

        var diamondGrant = new FakeDiamondGrant();
        var handler = CreateHandler(repository, new FakeAdMobSsvVerifier(verified: true), diamondGrant);

        var result = await handler.Handle(Command(playerId, "tx-new"), CancellationToken.None);

        result.Outcome.Should().Be(RewardedAdGrantOutcome.DailyLimitReached.ToString());
        result.RemainingToday.Should().Be(0);
        diamondGrant.Grants.Should().BeEmpty();
        repository.Grants.Should().HaveCount(10);
    }

    [Test]
    public async Task Invalid_Signature_Returns_VerificationFailed_Without_Grant()
    {
        var playerId = Guid.NewGuid();
        var repository = new FakeRewardedAdGrantRepository();
        var diamondGrant = new FakeDiamondGrant();
        var handler = CreateHandler(repository, new FakeAdMobSsvVerifier(verified: false), diamondGrant);

        var result = await handler.Handle(Command(playerId, "tx-1"), CancellationToken.None);

        result.Outcome.Should().Be(RewardedAdGrantOutcome.VerificationFailed.ToString());
        diamondGrant.Grants.Should().BeEmpty();
        repository.Grants.Should().BeEmpty();
    }

    private static GrantRewardedAdRewardCommand Command(Guid playerId, string transactionId) =>
        new(
            userId: playerId.ToString(),
            transactionId: transactionId,
            adUnitId: "ca-app-pub-test/rewarded",
            customData: null,
            rewardAmount: 1,
            rewardItem: "coins",
            keyId: "3335741209",
            signature: "signature-value",
            signedContent: "ad_network=...&transaction_id=" + transactionId);

    private static GrantRewardedAdRewardCommandHandler CreateHandler(
        FakeRewardedAdGrantRepository repository,
        FakeAdMobSsvVerifier verifier,
        FakeDiamondGrant diamondGrant) =>
        new(
            repository,
            verifier,
            diamondGrant,
            new FakeAdsConfigurationService(rewardedDiamondAmount: 5, rewardedDailyLimit: 10),
            new FixedClock(Now));

    private sealed class FakeRewardedAdGrantRepository : IRewardedAdGrantRepository
    {
        private readonly List<RewardedAdGrant> _grants = [];

        public IReadOnlyList<RewardedAdGrant> Grants => _grants;

        public void Seed(RewardedAdGrant grant) => _grants.Add(grant);

        public Task<RewardedAdGrant?> GetByTransactionIdAsync(
            string transactionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_grants.SingleOrDefault(x => x.TransactionId == transactionId));

        public Task<int> CountForPlayerSinceAsync(
            Guid playerId,
            DateTime sinceUtc,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_grants.Count(x => x.PlayerId == playerId && x.GrantedOn >= sinceUtc));

        public Task AddAsync(RewardedAdGrant grant, CancellationToken cancellationToken = default)
        {
            _grants.Add(grant);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAdMobSsvVerifier : IAdMobSsvVerifier
    {
        private readonly bool _verified;

        public FakeAdMobSsvVerifier(bool verified)
        {
            _verified = verified;
        }

        public Task<AdMobSsvVerificationResult> VerifyAsync(
            AdMobSsvVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_verified
                ? AdMobSsvVerificationResult.Verified()
                : AdMobSsvVerificationResult.Failed("invalid signature"));
    }

    private sealed class FakeDiamondGrant : IDiamondGrant
    {
        public List<(Guid PlayerId, int Amount)> Grants { get; } = [];

        public Task GrantAsync(Guid playerId, int amount, CancellationToken cancellationToken = default)
        {
            Grants.Add((playerId, amount));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAdsConfigurationService : IAdsConfigurationService
    {
        public FakeAdsConfigurationService(int rewardedDiamondAmount, int rewardedDailyLimit)
        {
            RewardedDiamondAmount = rewardedDiamondAmount;
            RewardedDailyLimit = rewardedDailyLimit;
        }

        public int RewardedDiamondAmount { get; }

        public int RewardedDailyLimit { get; }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}
