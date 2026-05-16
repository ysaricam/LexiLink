using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestGoalMustBePositiveRule : IBusinessRule
{
    private readonly int _goal;

    public QuestGoalMustBePositiveRule(int goal)
    {
        _goal = goal;
    }

    public bool IsBroken() => _goal <= 0;

    public string Message => "Quest goal must be positive.";
}
