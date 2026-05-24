using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestRewardMustBePositiveRule : IBusinessRule
{
    private readonly int _reward;

    public QuestRewardMustBePositiveRule(int reward)
    {
        _reward = reward;
    }

    public bool IsBroken() => _reward <= 0;

    public string Message => "Quest reward must be positive.";
}
