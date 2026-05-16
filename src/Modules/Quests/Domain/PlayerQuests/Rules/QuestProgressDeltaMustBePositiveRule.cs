using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestProgressDeltaMustBePositiveRule : IBusinessRule
{
    private readonly int _delta;

    public QuestProgressDeltaMustBePositiveRule(int delta)
    {
        _delta = delta;
    }

    public bool IsBroken() => _delta <= 0;

    public string Message => "Quest progress delta must be positive.";
}
