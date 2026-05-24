using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestThresholdMustBePositiveRule : IBusinessRule
{
    private readonly int _threshold;

    public QuestThresholdMustBePositiveRule(int threshold)
    {
        _threshold = threshold;
    }

    public bool IsBroken() => _threshold <= 0;

    public string Message => "Quest threshold must be positive.";
}
