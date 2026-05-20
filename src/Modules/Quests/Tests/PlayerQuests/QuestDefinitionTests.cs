using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;
using LexiLink.Modules.Quests.Tests.SeedWork;

namespace LexiLink.Modules.Quests.Tests.PlayerQuests;

[TestFixture]
public class QuestDefinitionTests : TestBase
{
    [Test]
    public void Create_Should_BuildActiveDefinition_AndPublishCreatedEvent()
    {
        var definition = QuestDefinition.Create(
            QuestType.ThreeGamesCompleted,
            QuestCadence.OneTime,
            goal: 3,
            rewardAmount: 5,
            prerequisiteQuestType: null);

        definition.QuestType.Should().Be(QuestType.ThreeGamesCompleted);
        definition.Cadence.Should().Be(QuestCadence.OneTime);
        definition.Goal.Should().Be(3);
        definition.RewardAmount.Should().Be(5);
        definition.PrerequisiteQuestType.Should().BeNull();
        definition.IsActive.Should().BeTrue();

        var created = AssertPublishedDomainEvent<QuestDefinitionCreatedDomainEvent>(definition);
        created.QuestDefinitionId.Should().Be(definition.Id.Value);
        created.QuestType.Should().Be(nameof(QuestType.ThreeGamesCompleted));
        created.Cadence.Should().Be(nameof(QuestCadence.OneTime));
        created.Goal.Should().Be(3);
        created.RewardAmount.Should().Be(5);
        created.PrerequisiteQuestType.Should().BeNull();
    }

    [Test]
    public void Create_Should_CarryPrerequisite_WhenProvided()
    {
        var definition = QuestDefinition.Create(
            QuestType.AccountLinked,
            QuestCadence.OneTime,
            goal: 1,
            rewardAmount: 5,
            prerequisiteQuestType: QuestType.ThreeGamesCompleted);

        definition.PrerequisiteQuestType.Should().Be(QuestType.ThreeGamesCompleted);
        var created = AssertPublishedDomainEvent<QuestDefinitionCreatedDomainEvent>(definition);
        created.PrerequisiteQuestType.Should().Be(nameof(QuestType.ThreeGamesCompleted));
    }

    [Test]
    public void Create_Should_RejectNonPositiveGoal()
    {
        AssertBrokenRule<QuestGoalMustBePositiveRule>(() => QuestDefinition.Create(
            QuestType.FirstGameCompleted,
            QuestCadence.OneTime,
            goal: 0,
            rewardAmount: 3,
            prerequisiteQuestType: null));
    }

    [Test]
    public void Create_Should_RejectNonPositiveReward()
    {
        AssertBrokenRule<QuestRewardAmountMustBePositiveRule>(() => QuestDefinition.Create(
            QuestType.FirstGameCompleted,
            QuestCadence.OneTime,
            goal: 1,
            rewardAmount: 0,
            prerequisiteQuestType: null));
    }

    [Test]
    public void Update_Should_ChangeTunableFields_AndPublishUpdatedEvent()
    {
        var definition = QuestDefinition.Create(
            QuestType.ThreeGamesCompleted,
            QuestCadence.OneTime,
            goal: 3,
            rewardAmount: 5,
            prerequisiteQuestType: null);

        definition.Update(goal: 5, rewardAmount: 10, prerequisiteQuestType: QuestType.FirstGameCompleted);

        definition.Goal.Should().Be(5);
        definition.RewardAmount.Should().Be(10);
        definition.PrerequisiteQuestType.Should().Be(QuestType.FirstGameCompleted);

        var updated = AssertPublishedDomainEvent<QuestDefinitionUpdatedDomainEvent>(definition);
        updated.Goal.Should().Be(5);
        updated.RewardAmount.Should().Be(10);
        updated.PrerequisiteQuestType.Should().Be(nameof(QuestType.FirstGameCompleted));
    }

    [Test]
    public void Update_Should_RejectInvalidGoal()
    {
        var definition = QuestDefinition.Create(
            QuestType.ThreeGamesCompleted, QuestCadence.OneTime, 3, 5, null);

        AssertBrokenRule<QuestGoalMustBePositiveRule>(() =>
            definition.Update(goal: -1, rewardAmount: 5, prerequisiteQuestType: null));
    }

    [Test]
    public void Deactivate_Should_FlipFlag_AndPublishEvent_OnFirstCall()
    {
        var definition = QuestDefinition.Create(
            QuestType.FirstGameCompleted, QuestCadence.OneTime, 1, 3, null);

        definition.Deactivate();

        definition.IsActive.Should().BeFalse();
        var evt = AssertPublishedDomainEvent<QuestDefinitionActivationChangedDomainEvent>(definition);
        evt.IsActive.Should().BeFalse();
    }

    [Test]
    public void Deactivate_Should_BeIdempotent_WhenAlreadyInactive()
    {
        var definition = QuestDefinition.Create(
            QuestType.FirstGameCompleted, QuestCadence.OneTime, 1, 3, null);
        definition.Deactivate();
        DomainEventsTestHelper.ClearAllDomainEvents(definition);

        definition.Deactivate();

        definition.IsActive.Should().BeFalse();
        AssertDomainEventNotPublished<QuestDefinitionActivationChangedDomainEvent>(definition);
    }

    [Test]
    public void Reactivate_Should_FlipFlag_AndPublishEvent()
    {
        var definition = QuestDefinition.Create(
            QuestType.FirstGameCompleted, QuestCadence.OneTime, 1, 3, null);
        definition.Deactivate();
        DomainEventsTestHelper.ClearAllDomainEvents(definition);

        definition.Reactivate();

        definition.IsActive.Should().BeTrue();
        var evt = AssertPublishedDomainEvent<QuestDefinitionActivationChangedDomainEvent>(definition);
        evt.IsActive.Should().BeTrue();
    }
}
