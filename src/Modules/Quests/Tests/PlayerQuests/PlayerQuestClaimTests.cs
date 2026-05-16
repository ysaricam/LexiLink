using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

[TestFixture]
public class PlayerQuestClaimTests : PlayerQuestTestsBase
{
    [Test]
    public void Claim_FromReadyToClaim_TransitionsToClaimed()
    {
        var quest = Issue(goal: 1);
        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));

        var claimedAt = FixedIssuedAt.AddSeconds(10);
        quest.Claim(claimedAt);

        quest.State.Should().Be(QuestState.Claimed);
        quest.ClaimedAt.Should().Be(claimedAt);
    }

    [Test]
    public void Claim_RaisesPlayerQuestClaimedDomainEvent_WithRewardAmount()
    {
        var quest = Issue(goal: 1, rewardAmount: 7);
        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));

        quest.Claim(FixedIssuedAt.AddSeconds(10));

        var evt = AssertPublishedDomainEvent<PlayerQuestClaimedDomainEvent>(quest);
        evt.RewardAmount.Should().Be(7);
        evt.QuestType.Should().Be(quest.QuestType);
        evt.PlayerQuestId.Should().Be(quest.Id);
    }

    [Test]
    public void Claim_FromActive_BreaksQuestMustBeReadyToBeClaimedRule()
    {
        var quest = Issue();

        AssertBrokenRule<QuestMustBeReadyToBeClaimedRule>(() =>
            quest.Claim(FixedIssuedAt.AddSeconds(1)));
    }

    [Test]
    public void Claim_Twice_BreaksQuestMustBeReadyToBeClaimedRule()
    {
        var quest = Issue(goal: 1);
        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));
        quest.Claim(FixedIssuedAt.AddSeconds(10));

        AssertBrokenRule<QuestMustBeReadyToBeClaimedRule>(() =>
            quest.Claim(FixedIssuedAt.AddSeconds(20)));
    }

    [Test]
    public void Claim_AfterExpiry_BreaksQuestMustBeReadyToBeClaimedRule()
    {
        var expiresAt = FixedIssuedAt.AddHours(1);
        var quest = Issue(goal: 1, expiresAt: expiresAt);
        quest.RecordProgress(1, FixedIssuedAt.AddSeconds(1));

        AssertBrokenRule<QuestMustBeReadyToBeClaimedRule>(() =>
            quest.Claim(expiresAt.AddSeconds(1)));

        quest.State.Should().Be(QuestState.Expired);
    }
}
