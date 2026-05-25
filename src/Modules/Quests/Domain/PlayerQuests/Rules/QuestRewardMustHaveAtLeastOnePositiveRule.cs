using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

/// <summary>
/// Quest rewards expanded in Sprint H from a single int to
/// (EnergyReward, HintReward). Both must be non-negative, and at
/// least one must be positive — a zero-zero reward would be a quest
/// the player has no incentive to complete.
/// </summary>
public class QuestRewardMustHaveAtLeastOnePositiveRule : IBusinessRule
{
    private readonly int _energyReward;
    private readonly int _hintReward;

    public QuestRewardMustHaveAtLeastOnePositiveRule(int energyReward, int hintReward)
    {
        _energyReward = energyReward;
        _hintReward = hintReward;
    }

    public bool IsBroken() => _energyReward < 0 || _hintReward < 0 || (_energyReward == 0 && _hintReward == 0);

    public string Message =>
        "Quest reward must be non-negative and at least one of EnergyReward / HintReward must be positive.";
}
