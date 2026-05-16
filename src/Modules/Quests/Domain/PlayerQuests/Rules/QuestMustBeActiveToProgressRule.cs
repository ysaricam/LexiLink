using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestMustBeActiveToProgressRule : IBusinessRule
{
    private readonly QuestState _state;

    public QuestMustBeActiveToProgressRule(QuestState state)
    {
        _state = state;
    }

    public bool IsBroken() => _state != QuestState.Active;

    public string Message => "Quest progress can only be recorded while it is active.";
}
