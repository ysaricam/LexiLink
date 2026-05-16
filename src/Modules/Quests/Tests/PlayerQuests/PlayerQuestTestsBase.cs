using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Tests.SeedWork;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

public abstract class PlayerQuestTestsBase : TestBase
{
    protected static readonly Guid SamplePlayerId =
        new("11111111-1111-1111-1111-111111111111");

    protected static readonly DateTime FixedIssuedAt =
        new(2026, 5, 14, 9, 0, 0, DateTimeKind.Utc);

    protected const QuestType SampleQuestType = QuestType.ThreeGamesCompleted;
    protected const int SampleGoal = 3;
    protected const int SampleRewardAmount = 5;

    protected static PlayerQuest Issue(
        QuestType questType = SampleQuestType,
        int goal = SampleGoal,
        int rewardAmount = SampleRewardAmount,
        DateTime? issuedAt = null,
        DateTime? expiresAt = null)
    {
        return PlayerQuest.IssueFor(
            SamplePlayerId,
            questType,
            goal,
            rewardAmount,
            issuedAt ?? FixedIssuedAt,
            expiresAt);
    }
}
