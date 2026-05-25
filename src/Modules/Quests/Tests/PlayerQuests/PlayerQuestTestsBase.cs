using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Tests.SeedWork;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

public abstract class PlayerQuestTestsBase : TestBase
{
    protected static readonly Guid SamplePlayerId =
        new("11111111-1111-1111-1111-111111111111");

    protected static readonly QuestDefinitionId SampleDefinitionId =
        new(new Guid("22222222-2222-2222-2222-222222222222"));

    protected static readonly DateTime FixedIssuedAt =
        new(2026, 5, 14, 9, 0, 0, DateTimeKind.Utc);

    protected const int SampleBaselineSnapshot = 0;
    protected const int SampleEnergyReward = 5;
    protected const int SampleHintReward = 0;

    protected static PlayerQuest Issue(
        QuestDefinitionId? questDefinitionId = null,
        int baselineSnapshot = SampleBaselineSnapshot,
        DateTime? issuedAt = null,
        DateTime? expiresAt = null)
    {
        return PlayerQuest.IssueFor(
            SamplePlayerId,
            questDefinitionId ?? SampleDefinitionId,
            baselineSnapshot,
            issuedAt ?? FixedIssuedAt,
            expiresAt);
    }
}
