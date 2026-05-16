using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestRewardAmountMustBePositiveRule : IBusinessRule
{
    private readonly int _rewardAmount;

    public QuestRewardAmountMustBePositiveRule(int rewardAmount)
    {
        _rewardAmount = rewardAmount;
    }

    public bool IsBroken() => _rewardAmount <= 0;

    public string Message => "Quest reward amount must be positive.";
}
