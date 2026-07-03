using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public sealed class QuestEnergyRewardMustHaveCapacityRule : IBusinessRule
{
    private readonly int _remainingEnergyReward;
    private readonly int _grantedEnergyReward;
    private readonly bool _hasNonEnergyRewardToClaim;

    internal QuestEnergyRewardMustHaveCapacityRule(
        int remainingEnergyReward,
        int grantedEnergyReward,
        bool hasNonEnergyRewardToClaim)
    {
        _remainingEnergyReward = remainingEnergyReward;
        _grantedEnergyReward = grantedEnergyReward;
        _hasNonEnergyRewardToClaim = hasNonEnergyRewardToClaim;
    }

    public bool IsBroken() =>
        _remainingEnergyReward > 0 &&
        _grantedEnergyReward <= 0 &&
        !_hasNonEnergyRewardToClaim;

    public string Message => "Energy reward cannot be claimed while the player's energy is full.";
}
