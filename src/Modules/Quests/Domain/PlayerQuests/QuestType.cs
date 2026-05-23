namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

public enum QuestType
{
    FirstGameCompleted = 1,
    ThreeGamesCompleted = 2,
    AccountLinked = 3,
    DailyThreeGames = 4,

    // Placeholder slots. Not seeded, no event-handler behavior — admin
    // creates a definition with one of these, then issues + claims
    // manually via the admin endpoints. Used to let the create-quest
    // flow be exercised without changing the catalog of known game
    // events.
    Custom1 = 101,
    Custom2 = 102,
    Custom3 = 103,
}
