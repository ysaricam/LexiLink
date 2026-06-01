using FluentValidation;

namespace LexiLink.Modules.Ads.Application.RewardedAdGrants.GrantRewardedAdReward;

internal sealed class GrantRewardedAdRewardCommandValidator
    : AbstractValidator<GrantRewardedAdRewardCommand>
{
    public GrantRewardedAdRewardCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.SignedContent).NotEmpty();
        RuleFor(x => x.Signature).NotEmpty();
        RuleFor(x => x.KeyId).NotEmpty();
    }
}
