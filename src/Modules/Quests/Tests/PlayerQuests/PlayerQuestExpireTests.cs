using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

[TestFixture]
public class PlayerQuestExpireTests : PlayerQuestTestsBase
{
    [Test]
    public void ExpireIfPast_WhenNoExpiry_DoesNothing()
    {
        var quest = Issue();

        quest.ExpireIfPast(FixedIssuedAt.AddDays(365));

        quest.State.Should().Be(QuestState.Active);
    }

    [Test]
    public void ExpireIfPast_BeforeExpiry_DoesNothing()
    {
        var expiresAt = FixedIssuedAt.AddHours(1);
        var quest = Issue(expiresAt: expiresAt);

        quest.ExpireIfPast(expiresAt.AddSeconds(-1));

        quest.State.Should().Be(QuestState.Active);
    }

    [Test]
    public void ExpireIfPast_AtOrAfterExpiry_FromActive_TransitionsToExpired()
    {
        var expiresAt = FixedIssuedAt.AddHours(1);
        var quest = Issue(expiresAt: expiresAt);

        quest.ExpireIfPast(expiresAt);

        quest.State.Should().Be(QuestState.Expired);
    }

    [Test]
    public void ExpireIfPast_AfterClaimed_DoesNotChangeState()
    {
        var expiresAt = FixedIssuedAt.AddHours(1);
        var quest = Issue(goal: 1, expiresAt: expiresAt);
        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));
        quest.Claim(FixedIssuedAt.AddSeconds(10));

        quest.ExpireIfPast(expiresAt.AddDays(1));

        quest.State.Should().Be(QuestState.Claimed);
    }

    [Test]
    public void ExpireIfPast_FromReadyToClaim_StillTransitionsToExpired()
    {
        var expiresAt = FixedIssuedAt.AddHours(1);
        var quest = Issue(goal: 1, expiresAt: expiresAt);
        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));

        quest.ExpireIfPast(expiresAt.AddSeconds(1));

        quest.State.Should().Be(QuestState.Expired);
    }
}
