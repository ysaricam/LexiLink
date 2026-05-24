using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

/// <summary>
/// Caller-supplied result: the command handler walks the prerequisite
/// chain via <see cref="IQuestDefinitionRepository"/> before invoking
/// Create / Update and passes <c>true</c> if the proposed prerequisite
/// would (eventually) point back at the definition being created or
/// updated. The Domain aggregate has no repository access, so this rule
/// stays parametric on a boolean.
/// </summary>
public class QuestPrerequisiteMustNotCreateCycleRule : IBusinessRule
{
    private readonly bool _wouldCreateCycle;

    public QuestPrerequisiteMustNotCreateCycleRule(bool wouldCreateCycle)
    {
        _wouldCreateCycle = wouldCreateCycle;
    }

    public bool IsBroken() => _wouldCreateCycle;

    public string Message => "Quest prerequisite chain must not create a cycle.";
}
