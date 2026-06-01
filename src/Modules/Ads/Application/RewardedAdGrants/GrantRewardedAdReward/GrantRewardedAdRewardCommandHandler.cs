using LexiLink.Common.Application.Time;
using LexiLink.Modules.Ads.Application.Configuration.Commands;
using LexiLink.Modules.Ads.Application.Configuration.Verification;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants;
using LexiLink.Modules.Ads.Domain.RewardedAdGrants.Rules;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;

namespace LexiLink.Modules.Ads.Application.RewardedAdGrants.GrantRewardedAdReward;

internal sealed class GrantRewardedAdRewardCommandHandler
    : ICommandHandler<GrantRewardedAdRewardCommand, GrantRewardedAdRewardResultDto>
{
    private readonly IRewardedAdGrantRepository _repository;
    private readonly IAdMobSsvVerifier _verifier;
    private readonly IDiamondGrant _diamondGrant;
    private readonly IAdsConfigurationService _configuration;
    private readonly IClock _clock;

    internal GrantRewardedAdRewardCommandHandler(
        IRewardedAdGrantRepository repository,
        IAdMobSsvVerifier verifier,
        IDiamondGrant diamondGrant,
        IAdsConfigurationService configuration,
        IClock clock)
    {
        _repository = repository;
        _verifier = verifier;
        _diamondGrant = diamondGrant;
        _configuration = configuration;
        _clock = clock;
    }

    public async Task<GrantRewardedAdRewardResultDto> Handle(
        GrantRewardedAdRewardCommand request,
        CancellationToken cancellationToken)
    {
        var limit = _configuration.RewardedDailyLimit;
        var amount = _configuration.RewardedDiamondAmount;

        // Idempotency first: a verified transaction was already granted, so a
        // replayed callback short-circuits without re-verifying or re-granting.
        var existing = await _repository.GetByTransactionIdAsync(request.TransactionId, cancellationToken);
        if (existing is not null)
        {
            var existingGrantsToday = await CountGrantsTodayAsync(existing.PlayerId, cancellationToken);
            return Result(
                RewardedAdGrantOutcome.AlreadyGranted,
                existing.DiamondAmount,
                existingGrantsToday,
                limit);
        }

        // Signature is the forgery gate. A client-reported reward alone never
        // grants Diamond; only AdMob's verified callback does.
        var verification = await _verifier.VerifyAsync(
            new AdMobSsvVerificationRequest(
                request.SignedContent,
                request.Signature,
                request.KeyId,
                request.TransactionId,
                request.UserId),
            cancellationToken);

        if (!verification.IsVerified || !Guid.TryParse(request.UserId, out var playerId))
        {
            return Result(RewardedAdGrantOutcome.VerificationFailed, diamondAmount: 0, grantsToday: 0, limit);
        }

        var grantsToday = await CountGrantsTodayAsync(playerId, cancellationToken);
        if (new RewardedAdDailyLimitRule(grantsToday, limit).IsBroken())
        {
            // Hitting the cap is a benign "no reward" outcome, not a failure.
            return Result(RewardedAdGrantOutcome.DailyLimitReached, diamondAmount: 0, grantsToday, limit);
        }

        await _diamondGrant.GrantAsync(playerId, amount, cancellationToken);

        var grant = RewardedAdGrant.Create(playerId, amount, request.TransactionId, _clock.UtcNow);
        await _repository.AddAsync(grant, cancellationToken);

        return Result(RewardedAdGrantOutcome.Granted, amount, grantsToday + 1, limit);
    }

    private Task<int> CountGrantsTodayAsync(Guid playerId, CancellationToken cancellationToken) =>
        _repository.CountForPlayerSinceAsync(playerId, _clock.UtcNow.Date, cancellationToken);

    private static GrantRewardedAdRewardResultDto Result(
        RewardedAdGrantOutcome outcome,
        int diamondAmount,
        int grantsToday,
        int dailyLimit) =>
        new(
            outcome.ToString(),
            diamondAmount,
            grantsToday,
            dailyLimit,
            Math.Max(0, dailyLimit - grantsToday));
}
