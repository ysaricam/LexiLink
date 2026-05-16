using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestMustBeReadyToBeClaimedRule : IBusinessRule
{
    private readonly QuestState _state;

    public QuestMustBeReadyToBeClaimedRule(QuestState state)
    {
        _state = state;
    }

    public bool IsBroken() => _state != QuestState.ReadyToClaim;

    public string Message => "Quest reward can only be claimed when the quest is ready to claim.";
}
