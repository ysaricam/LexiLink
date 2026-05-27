using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

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
            energyReward: SampleEnergyReward,
            hintReward: SampleHintReward,
            undoReward: SampleUndoReward,
            resetReward: SampleResetReward,
            diamondReward: 0);

        quest.State.Should().Be(QuestState.Claimed);
        quest.ClaimedAt.Should().Be(claimedAt);
    }

    [Test]
    public void Claim_RaisesPlayerQuestClaimedDomainEvent_CarryingAllRewards()
    {
        var quest = Issue();

        quest.Claim(FixedIssuedAt.AddSeconds(10), isReadyToClaim: true,
            energyReward: 7,
            hintReward: 3,
            undoReward: 2,
            resetReward: 1,
            diamondReward: 0);

        var evt = AssertPublishedDomainEvent<PlayerQuestClaimedDomainEvent>(quest);
        evt.EnergyReward.Should().Be(7);
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
                energyReward: SampleEnergyReward,
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
            energyReward: SampleEnergyReward,
            hintReward: SampleHintReward,
            undoReward: SampleUndoReward,
            resetReward: SampleResetReward,
            diamondReward: 0);

        AssertBrokenRule<QuestMustBeReadyToBeClaimedRule>(() =>
            quest.Claim(FixedIssuedAt.AddSeconds(20), isReadyToClaim: true,
                energyReward: SampleEnergyReward,
                hintReward: SampleHintReward,
                undoReward: SampleUndoReward,
                resetReward: SampleResetReward,
                diamondReward: SampleDiamondReward));
    }
}
