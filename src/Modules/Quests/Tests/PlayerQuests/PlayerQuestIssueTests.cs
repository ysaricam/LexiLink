using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

[TestFixture]
public class PlayerQuestIssueTests : PlayerQuestTestsBase
{
    [Test]
    public void IssueFor_WithValidValues_StartsActiveAtZeroProgress()
    {
        var quest = Issue();

        quest.PlayerId.Should().Be(SamplePlayerId);
        quest.QuestType.Should().Be(SampleQuestType);
        quest.Goal.Should().Be(SampleGoal);
        quest.RewardAmount.Should().Be(SampleRewardAmount);
        quest.Progress.Should().Be(0);
        quest.State.Should().Be(QuestState.Active);
        quest.IssuedAt.Should().Be(FixedIssuedAt);
        quest.CompletedAt.Should().BeNull();
        quest.ClaimedAt.Should().BeNull();
        quest.ExpiresAt.Should().BeNull();
    }

    [Test]
    public void IssueFor_RaisesPlayerQuestIssuedDomainEvent()
    {
        var quest = Issue();

        var evt = AssertPublishedDomainEvent<PlayerQuestIssuedDomainEvent>(quest);
        evt.PlayerId.Should().Be(SamplePlayerId);
        evt.QuestType.Should().Be(SampleQuestType);
        evt.PlayerQuestId.Should().Be(quest.Id);
    }

    [Test]
    public void IssueFor_WhenGoalIsZero_BreaksQuestGoalMustBePositiveRule()
    {
        AssertBrokenRule<QuestGoalMustBePositiveRule>(() => Issue(goal: 0));
    }

    [Test]
    public void IssueFor_WhenGoalIsNegative_BreaksQuestGoalMustBePositiveRule()
    {
        AssertBrokenRule<QuestGoalMustBePositiveRule>(() => Issue(goal: -1));
    }

    [Test]
    public void IssueFor_WhenRewardAmountIsZero_BreaksQuestRewardAmountMustBePositiveRule()
    {
        AssertBrokenRule<QuestRewardAmountMustBePositiveRule>(() => Issue(rewardAmount: 0));
    }

    [Test]
    public void IssueFor_CapturesExpiresAtWhenProvided()
    {
        var expiresAt = FixedIssuedAt.AddDays(1);
        var quest = Issue(expiresAt: expiresAt);

        quest.ExpiresAt.Should().Be(expiresAt);
    }
}
