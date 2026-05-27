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
            energyReward: 5,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot,
            prerequisiteWouldCreateCycle: false);

        definition.Name.Should().Be("Üç Oyun");
        definition.Description.Should().Be("3 oyun tamamla");
        definition.Trigger.Should().Be(QuestTrigger.GameCompletedTotal);
        definition.Threshold.Should().Be(3);
        definition.EnergyReward.Should().Be(5);
        definition.HintReward.Should().Be(0);
        definition.UndoReward.Should().Be(0);
        definition.ResetReward.Should().Be(0);
        definition.DiamondReward.Should().Be(0);
        definition.PrerequisiteQuestDefinitionId.Should().BeNull();
        definition.ProgressBaseline.Should().Be(ProgressBaseline.FromSnapshot);
        definition.IsActive.Should().BeTrue();

        var created = AssertPublishedDomainEvent<QuestDefinitionCreatedDomainEvent>(definition);
        created.QuestDefinitionId.Should().Be(definition.Id.Value);
        created.Name.Should().Be("Üç Oyun");
        created.Trigger.Should().Be(nameof(QuestTrigger.GameCompletedTotal));
        created.Threshold.Should().Be(3);
        created.EnergyReward.Should().Be(5);
        created.HintReward.Should().Be(0);
        created.UndoReward.Should().Be(0);
        created.ResetReward.Should().Be(0);
        created.DiamondReward.Should().Be(0);
        created.PrerequisiteQuestDefinitionId.Should().BeNull();
        created.ProgressBaseline.Should().Be(nameof(ProgressBaseline.FromSnapshot));
    }

    [Test]
    public void Create_Should_AllowHintOnlyReward()
    {
        var definition = Create(energyReward: 0, hintReward: 2);

        definition.EnergyReward.Should().Be(0);
        definition.HintReward.Should().Be(2);
    }

    [Test]
    public void Create_Should_AllowUndoOnlyReward()
    {
        var definition = Create(energyReward: 0, hintReward: 0, undoReward: 2);

        definition.EnergyReward.Should().Be(0);
        definition.HintReward.Should().Be(0);
        definition.UndoReward.Should().Be(2);
        definition.ResetReward.Should().Be(0);
    }

    [Test]
    public void Create_Should_AllowResetOnlyReward()
    {
        var definition = Create(energyReward: 0, hintReward: 0, resetReward: 1,
            diamondReward: 0);

        definition.EnergyReward.Should().Be(0);
        definition.HintReward.Should().Be(0);
        definition.UndoReward.Should().Be(0);
        definition.ResetReward.Should().Be(1);
        definition.DiamondReward.Should().Be(0);
    }

    [Test]
    public void Create_Should_AllowDiamondOnlyReward()
    {
        var definition = Create(energyReward: 0, hintReward: 0, diamondReward: 3);

        definition.EnergyReward.Should().Be(0);
        definition.HintReward.Should().Be(0);
        definition.UndoReward.Should().Be(0);
        definition.ResetReward.Should().Be(0);
        definition.DiamondReward.Should().Be(3);
    }

    [Test]
    public void Create_Should_AllowMixedReward()
    {
        var definition = Create(energyReward: 5, hintReward: 2, undoReward: 1, resetReward: 1,
            diamondReward: 0);

        definition.EnergyReward.Should().Be(5);
        definition.HintReward.Should().Be(2);
        definition.UndoReward.Should().Be(1);
        definition.ResetReward.Should().Be(1);
        definition.DiamondReward.Should().Be(0);
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
            energyReward: 5,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
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
    public void Create_Should_RejectAllRewardsZero()
    {
        AssertBrokenRule<QuestRewardMustHaveAtLeastOnePositiveRule>(() =>
            Create(energyReward: 0, hintReward: 0));
    }

    [Test]
    public void Create_Should_RejectNegativeEnergyReward()
    {
        AssertBrokenRule<QuestRewardMustHaveAtLeastOnePositiveRule>(() =>
            Create(energyReward: -1, hintReward: 5));
    }

    [Test]
    public void Create_Should_RejectNegativeHintReward()
    {
        AssertBrokenRule<QuestRewardMustHaveAtLeastOnePositiveRule>(() =>
            Create(energyReward: 5, hintReward: -1));
    }

    [Test]
    public void Create_Should_RejectNegativeUndoReward()
    {
        AssertBrokenRule<QuestRewardMustHaveAtLeastOnePositiveRule>(() =>
            Create(energyReward: 5, undoReward: -1));
    }

    [Test]
    public void Create_Should_RejectNegativeResetReward()
    {
        AssertBrokenRule<QuestRewardMustHaveAtLeastOnePositiveRule>(() =>
            Create(energyReward: 5, resetReward: -1));
    }

    [Test]
    public void Create_Should_RejectNegativeDiamondReward()
    {
        AssertBrokenRule<QuestRewardMustHaveAtLeastOnePositiveRule>(() =>
            Create(energyReward: 5, diamondReward: -1));
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
            energyReward: 10,
            hintReward: 2,
            undoReward: 3,
            resetReward: 4,
            diamondReward: 0,
            prerequisiteQuestDefinitionId: newPrereq,
            progressBaseline: ProgressBaseline.FromExistingTotal,
            prerequisiteWouldCreateCycle: false);

        definition.Description.Should().Be("yeni açıklama");
        definition.Threshold.Should().Be(5);
        definition.EnergyReward.Should().Be(10);
        definition.HintReward.Should().Be(2);
        definition.UndoReward.Should().Be(3);
        definition.ResetReward.Should().Be(4);
        definition.DiamondReward.Should().Be(0);
        definition.PrerequisiteQuestDefinitionId.Should().Be(newPrereq);
        definition.ProgressBaseline.Should().Be(ProgressBaseline.FromExistingTotal);

        var updated = AssertPublishedDomainEvent<QuestDefinitionUpdatedDomainEvent>(definition);
        updated.Description.Should().Be("yeni açıklama");
        updated.Threshold.Should().Be(5);
        updated.EnergyReward.Should().Be(10);
        updated.HintReward.Should().Be(2);
        updated.UndoReward.Should().Be(3);
        updated.ResetReward.Should().Be(4);
        updated.DiamondReward.Should().Be(0);
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
            energyReward: 5,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
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
            energyReward: 5,
            hintReward: 0,
            undoReward: 0,
            resetReward: 0,
            diamondReward: 0,
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
        int energyReward = 5,
        int hintReward = 0,
        int undoReward = 0,
        int resetReward = 0,
        int diamondReward = 0,
        QuestDefinitionId? prerequisiteQuestDefinitionId = null,
        ProgressBaseline progressBaseline = ProgressBaseline.FromSnapshot,
        bool prerequisiteWouldCreateCycle = false) =>
        QuestDefinition.Create(
            name,
            description,
            trigger,
            threshold,
            energyReward,
            hintReward,
            undoReward,
            resetReward,
            diamondReward,
            prerequisiteQuestDefinitionId,
            progressBaseline,
            prerequisiteWouldCreateCycle);
}
