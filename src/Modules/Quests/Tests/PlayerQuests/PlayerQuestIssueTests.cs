using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

[TestFixture]
public class PlayerQuestIssueTests : PlayerQuestTestsBase
{
    [Test]
    public void IssueFor_WithValidValues_StartsActive_CapturesBaselineSnapshot()
    {
        var quest = Issue(baselineSnapshot: 7);

        quest.PlayerId.Should().Be(SamplePlayerId);
        quest.QuestDefinitionId.Should().Be(SampleDefinitionId);
        quest.ProgressBaselineSnapshot.Should().Be(7);
        quest.State.Should().Be(QuestState.Active);
        quest.IssuedAt.Should().Be(FixedIssuedAt);
        quest.ClaimedAt.Should().BeNull();
        quest.ExpiresAt.Should().BeNull();
    }

    [Test]
    public void IssueFor_RaisesPlayerQuestIssuedDomainEvent_CarryingDefinitionId()
    {
        var quest = Issue();

        var evt = AssertPublishedDomainEvent<PlayerQuestIssuedDomainEvent>(quest);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.QuestDefinitionId.Should().Be(SampleDefinitionId);
        evt.PlayerQuestId.Should().Be(quest.Id);
    }

    [Test]
    public void IssueFor_ClampsNegativeBaselineSnapshotToZero()
    {
        var quest = Issue(baselineSnapshot: -3);

        quest.ProgressBaselineSnapshot.Should().Be(0);
    }

    [Test]
    public void IssueFor_CapturesExpiresAtWhenProvided()
    {
        var expiresAt = FixedIssuedAt.AddDays(1);
        var quest = Issue(expiresAt: expiresAt);

        quest.ExpiresAt.Should().Be(expiresAt);
    }
}
