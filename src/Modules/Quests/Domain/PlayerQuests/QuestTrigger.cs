namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// What player counter a quest tracks. Fixed at three values — extending
/// requires a Domain change and a matching <see cref="QuestDefinition"/>-
/// aware counter read in the Application layer.
/// </summary>
public enum QuestTrigger
{
    GameCompletedTotal = 1,
    GameCompletedDaily = 2,
    AuthProviderLinked = 3,
}
