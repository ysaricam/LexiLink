using LexiLink.Common.Domain;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// Catalog entry describing how a quest is issued and rewarded. Fully
/// data-driven post Sprint Q1 — every field is admin-configurable
/// except <see cref="Name"/> and <see cref="Trigger"/>, which are fixed
/// at creation time so an admin re-name does not silently re-key
/// PlayerQuest history and a trigger swap does not invalidate baseline
/// snapshots.
///
/// Sprint H expanded the single reward int into Energy + Hint; UR5
/// adds Undo + Reset; D3 adds Diamond. All reward amounts are non-negative and at
/// least one must be positive. Each resource module consumes
/// <c>QuestClaimedIntegrationEvent</c> independently and grants its
/// portion.
/// </summary>
public sealed class QuestDefinition : Entity, IAggregateRoot
{
    public QuestDefinitionId Id { get; private set; }

    private string _name = null!;
    private string _description = null!;
    private QuestTrigger _trigger;
    private int _threshold;
    private int _energyReward;
    private int _hintReward;
    private int _undoReward;
    private int _resetReward;
    private int _diamondReward;
    private QuestDefinitionId? _prerequisiteQuestDefinitionId;
    private ProgressBaseline _progressBaseline;
    private bool _isActive;

    public string Name => _name;
    public string Description => _description;
    public QuestTrigger Trigger => _trigger;
    public int Threshold => _threshold;
    public int EnergyReward => _energyReward;
    public int HintReward => _hintReward;
    public int UndoReward => _undoReward;
    public int ResetReward => _resetReward;
    public int DiamondReward => _diamondReward;
    public QuestDefinitionId? PrerequisiteQuestDefinitionId => _prerequisiteQuestDefinitionId;
    public ProgressBaseline ProgressBaseline => _progressBaseline;
    public bool IsActive => _isActive;

    private QuestDefinition()
    {
        Id = null!;
    }

    private QuestDefinition(
        QuestDefinitionId id,
        string name,
        string description,
        QuestTrigger trigger,
        int threshold,
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward,
        int diamondReward,
        QuestDefinitionId? prerequisiteQuestDefinitionId,
        ProgressBaseline progressBaseline,
        bool prerequisiteWouldCreateCycle)
    {
        CheckRule(new QuestNameMustNotBeEmptyRule(name));
        CheckRule(new QuestNameMustNotExceedMaxLengthRule(name));
        CheckRule(new QuestDescriptionMustNotExceedMaxLengthRule(description));
        CheckRule(new QuestThresholdMustBePositiveRule(threshold));
        CheckRule(new QuestRewardMustHaveAtLeastOnePositiveRule(
            energyReward,
            hintReward,
            undoReward,
            resetReward,
            diamondReward));
        CheckRule(new QuestPrerequisiteMustNotCreateCycleRule(prerequisiteWouldCreateCycle));

        Id = id;
        _name = name.Trim();
        _description = description ?? string.Empty;
        _trigger = trigger;
        _threshold = threshold;
        _energyReward = energyReward;
        _hintReward = hintReward;
        _undoReward = undoReward;
        _resetReward = resetReward;
        _diamondReward = diamondReward;
        _prerequisiteQuestDefinitionId = prerequisiteQuestDefinitionId;
        _progressBaseline = progressBaseline;
        _isActive = true;

        AddDomainEvent(new QuestDefinitionCreatedDomainEvent(
            id.Value,
            _name,
            trigger.ToString(),
            threshold,
            energyReward,
            hintReward,
            undoReward,
            resetReward,
            diamondReward,
            prerequisiteQuestDefinitionId?.Value,
            progressBaseline.ToString()));
    }

    public static QuestDefinition Create(
        string name,
        string description,
        QuestTrigger trigger,
        int threshold,
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward,
        int diamondReward,
        QuestDefinitionId? prerequisiteQuestDefinitionId,
        ProgressBaseline progressBaseline,
        bool prerequisiteWouldCreateCycle)
    {
        return new QuestDefinition(
            new QuestDefinitionId(Guid.NewGuid()),
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

    /// <summary>
    /// Update mutable fields. Name and Trigger are immutable post-create
    /// — changing them would invalidate the meaning of existing
    /// PlayerQuest rows (snapshots are sized against the original
    /// threshold/trigger). Deactivate + create-new is the migration path.
    /// </summary>
    public void Update(
        string description,
        int threshold,
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward,
        int diamondReward,
        QuestDefinitionId? prerequisiteQuestDefinitionId,
        ProgressBaseline progressBaseline,
        bool prerequisiteWouldCreateCycle)
    {
        CheckRule(new QuestDescriptionMustNotExceedMaxLengthRule(description));
        CheckRule(new QuestThresholdMustBePositiveRule(threshold));
        CheckRule(new QuestRewardMustHaveAtLeastOnePositiveRule(
            energyReward,
            hintReward,
            undoReward,
            resetReward,
            diamondReward));
        CheckRule(new QuestPrerequisiteMustNotCreateCycleRule(prerequisiteWouldCreateCycle));

        _description = description ?? string.Empty;
        _threshold = threshold;
        _energyReward = energyReward;
        _hintReward = hintReward;
        _undoReward = undoReward;
        _resetReward = resetReward;
        _diamondReward = diamondReward;
        _prerequisiteQuestDefinitionId = prerequisiteQuestDefinitionId;
        _progressBaseline = progressBaseline;

        AddDomainEvent(new QuestDefinitionUpdatedDomainEvent(
            Id.Value,
            _description,
            threshold,
            energyReward,
            hintReward,
            undoReward,
            resetReward,
            diamondReward,
            prerequisiteQuestDefinitionId?.Value,
            progressBaseline.ToString()));
    }

    public void Deactivate()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;
        AddDomainEvent(new QuestDefinitionActivationChangedDomainEvent(Id.Value, false));
    }

    public void Reactivate()
    {
        if (_isActive)
        {
            return;
        }

        _isActive = true;
        AddDomainEvent(new QuestDefinitionActivationChangedDomainEvent(Id.Value, true));
    }
}
