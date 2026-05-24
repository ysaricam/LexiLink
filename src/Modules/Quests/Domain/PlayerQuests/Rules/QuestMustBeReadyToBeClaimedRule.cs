using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestMustBeReadyToBeClaimedRule : IBusinessRule
{
    private readonly QuestState _state;
    private readonly bool _isReadyToClaim;

    public QuestMustBeReadyToBeClaimedRule(QuestState state, bool isReadyToClaim)
    {
        _state = state;
        _isReadyToClaim = isReadyToClaim;
    }

    public bool IsBroken() => _state != QuestState.Active || !_isReadyToClaim;

    public string Message => "Quest reward can only be claimed when the quest is active and the threshold has been reached.";
}
