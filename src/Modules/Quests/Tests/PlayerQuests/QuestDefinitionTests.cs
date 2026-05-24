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
            name: "Üç Oyun",
            description: "3 oyun tamamla",
            trigger: QuestTrigger.GameCompletedTotal,
            threshold: 3,
            reward: 5,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot,
            prerequisiteWouldCreateCycle: false);

        definition.Name.Should().Be("Üç Oyun");
        definition.Description.Should().Be("3 oyun tamamla");
        definition.Trigger.Should().Be(QuestTrigger.GameCompletedTotal);
        definition.Threshold.Should().Be(3);
        definition.Reward.Should().Be(5);
        definition.PrerequisiteQuestDefinitionId.Should().BeNull();
        definition.ProgressBaseline.Should().Be(ProgressBaseline.FromSnapshot);
        definition.IsActive.Should().BeTrue();

        var created = AssertPublishedDomainEvent<QuestDefinitionCreatedDomainEvent>(definition);
        created.QuestDefinitionId.Should().Be(definition.Id.Value);
        created.Name.Should().Be("Üç Oyun");
        created.Trigger.Should().Be(nameof(QuestTrigger.GameCompletedTotal));
        created.Threshold.Should().Be(3);
        created.Reward.Should().Be(5);
        created.PrerequisiteQuestDefinitionId.Should().BeNull();
        created.ProgressBaseline.Should().Be(nameof(ProgressBaseline.FromSnapshot));
    }

    [Test]
    public void Create_Should_CarryPrerequisite_WhenProvided()
    {
        var prereqId = new QuestDefinitionId(Guid.NewGuid());

        var definition = QuestDefinition.Create(
            name: "Hesabını Bağla",
            description: "",
            trigger: QuestTrigger.AuthProviderLinked,
            threshold: 1,
            reward: 5,
            prerequisiteQuestDefinitionId: prereqId,
            progressBaseline: ProgressBaseline.FromSnapshot,
            prerequisiteWouldCreateCycle: false);

        definition.PrerequisiteQuestDefinitionId.Should().Be(prereqId);
        var created = AssertPublishedDomainEvent<QuestDefinitionCreatedDomainEvent>(definition);
        created.PrerequisiteQuestDefinitionId.Should().Be(prereqId.Value);
    }

    [Test]
    public void Create_Should_RejectEmptyName()
    {
        AssertBrokenRule<QuestNameMustNotBeEmptyRule>(() => Create(name: "  "));
    }

    [Test]
    public void Create_Should_RejectNameLongerThan64Chars()
    {
        AssertBrokenRule<QuestNameMustNotExceedMaxLengthRule>(() => Create(name: new string('x', 65)));
    }

    [Test]
    public void Create_Should_RejectDescriptionLongerThan256Chars()
    {
        AssertBrokenRule<QuestDescriptionMustNotExceedMaxLengthRule>(() =>
            Create(description: new string('x', 257)));
    }

    [Test]
    public void Create_Should_RejectNonPositiveThreshold()
    {
        AssertBrokenRule<QuestThresholdMustBePositiveRule>(() => Create(threshold: 0));
    }

    [Test]
    public void Create_Should_RejectNonPositiveReward()
    {
        AssertBrokenRule<QuestRewardMustBePositiveRule>(() => Create(reward: 0));
    }

    [Test]
    public void Create_Should_RejectCycleWhenHandlerSignalsIt()
    {
        AssertBrokenRule<QuestPrerequisiteMustNotCreateCycleRule>(() =>
            Create(prerequisiteWouldCreateCycle: true));
    }

    [Test]
    public void Update_Should_ChangeTunableFields_AndPublishUpdatedEvent()
    {
        var definition = Create();
        var newPrereq = new QuestDefinitionId(Guid.NewGuid());

        definition.Update(
            description: "yeni açıklama",
            threshold: 5,
            reward: 10,
            prerequisiteQuestDefinitionId: newPrereq,
            progressBaseline: ProgressBaseline.FromExistingTotal,
            prerequisiteWouldCreateCycle: false);

        definition.Description.Should().Be("yeni açıklama");
        definition.Threshold.Should().Be(5);
        definition.Reward.Should().Be(10);
        definition.PrerequisiteQuestDefinitionId.Should().Be(newPrereq);
        definition.ProgressBaseline.Should().Be(ProgressBaseline.FromExistingTotal);

        var updated = AssertPublishedDomainEvent<QuestDefinitionUpdatedDomainEvent>(definition);
        updated.Description.Should().Be("yeni açıklama");
        updated.Threshold.Should().Be(5);
        updated.Reward.Should().Be(10);
        updated.PrerequisiteQuestDefinitionId.Should().Be(newPrereq.Value);
        updated.ProgressBaseline.Should().Be(nameof(ProgressBaseline.FromExistingTotal));
    }

    [Test]
    public void Update_Should_RejectInvalidThreshold()
    {
        var definition = Create();

        AssertBrokenRule<QuestThresholdMustBePositiveRule>(() => definition.Update(
            description: "x",
            threshold: -1,
            reward: 5,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot,
            prerequisiteWouldCreateCycle: false));
    }

    [Test]
    public void Update_Should_RejectCycleWhenHandlerSignalsIt()
    {
        var definition = Create();

        AssertBrokenRule<QuestPrerequisiteMustNotCreateCycleRule>(() => definition.Update(
            description: "x",
            threshold: 1,
            reward: 5,
            prerequisiteQuestDefinitionId: new QuestDefinitionId(Guid.NewGuid()),
            progressBaseline: ProgressBaseline.FromSnapshot,
            prerequisiteWouldCreateCycle: true));
    }

    [Test]
    public void Deactivate_Should_FlipFlag_AndPublishEvent_OnFirstCall()
    {
        var definition = Create();

        definition.Deactivate();

        definition.IsActive.Should().BeFalse();
        var evt = AssertPublishedDomainEvent<QuestDefinitionActivationChangedDomainEvent>(definition);
        evt.IsActive.Should().BeFalse();
    }

    [Test]
    public void Deactivate_Should_BeIdempotent_WhenAlreadyInactive()
    {
        var definition = Create();
        definition.Deactivate();
        DomainEventsTestHelper.ClearAllDomainEvents(definition);

        definition.Deactivate();

        definition.IsActive.Should().BeFalse();
        AssertDomainEventNotPublished<QuestDefinitionActivationChangedDomainEvent>(definition);
    }

    [Test]
    public void Reactivate_Should_FlipFlag_AndPublishEvent()
    {
        var definition = Create();
        definition.Deactivate();
        DomainEventsTestHelper.ClearAllDomainEvents(definition);

        definition.Reactivate();

        definition.IsActive.Should().BeTrue();
        var evt = AssertPublishedDomainEvent<QuestDefinitionActivationChangedDomainEvent>(definition);
        evt.IsActive.Should().BeTrue();
    }

    private static QuestDefinition Create(
        string name = "Sample",
        string description = "desc",
        QuestTrigger trigger = QuestTrigger.GameCompletedTotal,
        int threshold = 3,
        int reward = 5,
        QuestDefinitionId? prerequisiteQuestDefinitionId = null,
        ProgressBaseline progressBaseline = ProgressBaseline.FromSnapshot,
        bool prerequisiteWouldCreateCycle = false) =>
        QuestDefinition.Create(
            name,
            description,
            trigger,
            threshold,
            reward,
            prerequisiteQuestDefinitionId,
            progressBaseline,
            prerequisiteWouldCreateCycle);
}
