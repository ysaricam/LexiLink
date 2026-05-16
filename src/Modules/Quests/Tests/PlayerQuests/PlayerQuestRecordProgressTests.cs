using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

[TestFixture]
public class PlayerQuestRecordProgressTests : PlayerQuestTestsBase
{
    [Test]
    public void RecordProgress_PartialProgress_StaysActive()
    {
        var quest = Issue(goal: 3);

        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));

        quest.Progress.Should().Be(1);
        quest.State.Should().Be(QuestState.Active);
        AssertDomainEventNotPublished<PlayerQuestCompletedDomainEvent>(quest);
    }

    [Test]
    public void RecordProgress_HittingGoal_TransitionsToReadyToClaim()
    {
        var quest = Issue(goal: 2);

        var completedAt = FixedIssuedAt.AddSeconds(5);
        quest.RecordProgress(2, completedAt);

        quest.Progress.Should().Be(2);
        quest.State.Should().Be(QuestState.ReadyToClaim);
        quest.CompletedAt.Should().Be(completedAt);

        var evt = AssertPublishedDomainEvent<PlayerQuestCompletedDomainEvent>(quest);
        evt.QuestType.Should().Be(quest.QuestType);
    }

    [Test]
    public void RecordProgress_DeltaThatWouldExceedGoal_ClampsToGoal()
    {
        var quest = Issue(goal: 3);

        quest.RecordProgress(99, FixedIssuedAt.AddSeconds(1));

        quest.Progress.Should().Be(3);
        quest.State.Should().Be(QuestState.ReadyToClaim);
    }

    [Test]
    public void RecordProgress_AfterReadyToClaim_BreaksQuestMustBeActiveToProgressRule()
    {
        var quest = Issue(goal: 1);
        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));

        AssertBrokenRule<QuestMustBeActiveToProgressRule>(() =>
            quest.RecordProgress(1, FixedIssuedAt.AddSeconds(2)));
    }

    [Test]
    public void RecordProgress_WithZeroDelta_BreaksQuestProgressDeltaMustBePositiveRule()
    {
        var quest = Issue();

        AssertBrokenRule<QuestProgressDeltaMustBePositiveRule>(() =>
            quest.RecordProgress(0, FixedIssuedAt.AddSeconds(1)));
    }

    [Test]
    public void RecordProgress_WithNegativeDelta_BreaksQuestProgressDeltaMustBePositiveRule()
    {
        var quest = Issue();

        AssertBrokenRule<QuestProgressDeltaMustBePositiveRule>(() =>
            quest.RecordProgress(-1, FixedIssuedAt.AddSeconds(1)));
    }

    [Test]
    public void RecordProgress_AfterExpiry_BreaksQuestMustBeActiveToProgressRule()
    {
        var expiresAt = FixedIssuedAt.AddHours(1);
        var quest = Issue(expiresAt: expiresAt);

        AssertBrokenRule<QuestMustBeActiveToProgressRule>(() =>
            quest.RecordProgress(1, expiresAt.AddSeconds(1)));

        quest.State.Should().Be(QuestState.Expired);
    }
}
