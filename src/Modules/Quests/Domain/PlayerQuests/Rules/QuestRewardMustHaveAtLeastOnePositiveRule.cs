using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

/// <summary>
/// Quest rewards are split by resource module. All five must be
/// non-negative, and at least one must be positive — an all-zero
/// reward would be a quest the player has no incentive to complete.
/// </summary>
public class QuestRewardMustHaveAtLeastOnePositiveRule : IBusinessRule
{
    private readonly int _energyReward;
    private readonly int _hintReward;
    private readonly int _undoReward;
    private readonly int _resetReward;
    private readonly int _diamondReward;

    public QuestRewardMustHaveAtLeastOnePositiveRule(
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward,
        int diamondReward)
    {
        _energyReward = energyReward;
        _hintReward = hintReward;
        _undoReward = undoReward;
        _resetReward = resetReward;
        _diamondReward = diamondReward;
    }

    public bool IsBroken() =>
        _energyReward < 0 ||
        _hintReward < 0 ||
        _undoReward < 0 ||
        _resetReward < 0 ||
        _diamondReward < 0 ||
        (_energyReward == 0 && _hintReward == 0 && _undoReward == 0 && _resetReward == 0 && _diamondReward == 0);

    public string Message =>
        "Quest reward must be non-negative and at least one reward amount must be positive.";
}
