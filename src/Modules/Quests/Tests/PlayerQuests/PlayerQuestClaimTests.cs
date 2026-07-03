using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;
using LexiLink.Modules.Quests.Tests.SeedWork;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

[TestFixture]
public class PlayerQuestClaimTests : PlayerQuestTestsBase
{
    [Test]
    public void Claim_FromActive_WhenReady_TransitionsToClaimed()
    {
        var quest = Issue();
        var claimedAt = FixedIssuedAt.AddSeconds(10);

        quest.Claim(claimedAt, isReadyToClaim: true,
            grantedEnergyReward: SampleEnergyReward,
            hintReward: SampleHintReward,
            undoReward: SampleUndoReward,
            resetReward: SampleResetReward,
            diamondReward: 0);

        quest.State.Should().Be(QuestState.Claimed);
        quest.ClaimedAt.Should().Be(claimedAt);
    }

    [Test]
    public void Claim_RaisesPlayerQuestClaimedDomainEvent_CarryingNonEnergyRewardsOnly()
    {
        var quest = Issue();

        quest.Claim(FixedIssuedAt.AddSeconds(10), isReadyToClaim: true,
            grantedEnergyReward: SampleEnergyReward,
            hintReward: 3,
            undoReward: 2,
            resetReward: 1,
            diamondReward: 0);

        var evt = AssertPublishedDomainEvent<PlayerQuestClaimedDomainEvent>(quest);
        evt.EnergyReward.Should().Be(0, "quest energy is granted synchronously and must not be delivered again by outbox");
        evt.HintReward.Should().Be(3);
        evt.UndoReward.Should().Be(2);
        evt.ResetReward.Should().Be(1);
        evt.DiamondReward.Should().Be(0);
        evt.QuestDefinitionId.Should().Be(quest.QuestDefinitionId);
        evt.PlayerQuestId.Should().Be(quest.Id);
    }

    [Test]
    public void Claim_WhenNotReady_BreaksQuestMustBeReadyToBeClaimedRule()
    {
        var quest = Issue();

        AssertBrokenRule<QuestMustBeReadyToBeClaimedRule>(() =>
            quest.Claim(FixedIssuedAt.AddSeconds(1), isReadyToClaim: false,
                grantedEnergyReward: SampleEnergyReward,
                hintReward: SampleHintReward,
                undoReward: SampleUndoReward,
                resetReward: SampleResetReward,
                diamondReward: SampleDiamondReward));
    }

    [Test]
    public void Claim_Twice_BreaksQuestMustBeReadyToBeClaimedRule()
    {
        var quest = Issue();
        quest.Claim(FixedIssuedAt.AddSeconds(10), isReadyToClaim: true,
            grantedEnergyReward: SampleEnergyReward,
            hintReward: SampleHintReward,
            undoReward: SampleUndoReward,
            resetReward: SampleResetReward,
            diamondReward: 0);

        AssertBrokenRule<QuestMustBeReadyToBeClaimedRule>(() =>
            quest.Claim(FixedIssuedAt.AddSeconds(20), isReadyToClaim: true,
                grantedEnergyReward: SampleEnergyReward,
                hintReward: SampleHintReward,
                undoReward: SampleUndoReward,
                resetReward: SampleResetReward,
                diamondReward: SampleDiamondReward));
    }

    [Test]
    public void Claim_WhenEnergyPartiallyGranted_KeepsQuestActiveWithRemainingEnergy()
    {
        var quest = Issue();

        quest.Claim(FixedIssuedAt.AddSeconds(10), isReadyToClaim: true,
            grantedEnergyReward: 4,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0);

        quest.State.Should().Be(QuestState.Active);
        quest.ClaimedAt.Should().BeNull();
        quest.RemainingEnergyReward.Should().Be(1);
        AssertDomainEventNotPublished<PlayerQuestClaimedDomainEvent>(quest);
    }

    [Test]
    public void Claim_WhenEnergyFullAndNoNonEnergyRewards_BreaksQuestEnergyRewardMustHaveCapacityRule()
    {
        var quest = Issue();

        AssertBrokenRule<QuestEnergyRewardMustHaveCapacityRule>(() =>
            quest.Claim(FixedIssuedAt.AddSeconds(10), isReadyToClaim: true,
                grantedEnergyReward: 0,
                hintReward: 0,
                undoReward: 0,
                resetReward: 0,
                diamondReward: 0));
    }

    [Test]
    public void Claim_WhenPartialEnergyAndNonEnergyRewards_DoesNotPublishNonEnergyTwice()
    {
        var quest = Issue();

        quest.Claim(FixedIssuedAt.AddSeconds(10), isReadyToClaim: true,
            grantedEnergyReward: 4,
            hintReward: 2,
            undoReward: 1,
            resetReward: 1,
            diamondReward: 3);

        quest.RemainingEnergyReward.Should().Be(1);
        var first = AssertPublishedDomainEvent<PlayerQuestClaimedDomainEvent>(quest);
        first.HintReward.Should().Be(2);
        first.UndoReward.Should().Be(1);
        first.ResetReward.Should().Be(1);
        first.DiamondReward.Should().Be(3);
        first.EnergyReward.Should().Be(0);
        DomainEventsTestHelper.ClearAllDomainEvents(quest);

        quest.Claim(FixedIssuedAt.AddSeconds(20), isReadyToClaim: true,
            grantedEnergyReward: 1,
            hintReward: 2,
            undoReward: 1,
            resetReward: 1,
            diamondReward: 3);

        quest.State.Should().Be(QuestState.Claimed);
        quest.RemainingEnergyReward.Should().Be(0);
        AssertDomainEventNotPublished<PlayerQuestClaimedDomainEvent>(quest);
    }
}
