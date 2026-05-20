using LexiLink.Common.Domain;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// Catalog entry describing how a quest is issued and rewarded.
/// Previously a hardcoded record; promoted to an aggregate in Slice B6
/// so admin tooling (B7) can create/update/deactivate definitions
/// without a code redeploy. Seed of the original four definitions is
/// delivered via DbUp SQL (deterministic ids) and intentionally bypasses
/// <see cref="Create"/> — those four are known-good and predate this
/// aggregate.
/// </summary>
public sealed class QuestDefinition : Entity, IAggregateRoot
{
    public QuestDefinitionId Id { get; private set; }

    private QuestType _questType;
    private QuestCadence _cadence;
    private int _goal;
    private int _rewardAmount;
    private QuestType? _prerequisiteQuestType;
    private bool _isActive;

    public QuestType QuestType => _questType;
    public QuestCadence Cadence => _cadence;
    public int Goal => _goal;
    public int RewardAmount => _rewardAmount;
    public QuestType? PrerequisiteQuestType => _prerequisiteQuestType;
    public bool IsActive => _isActive;

    private QuestDefinition()
    {
        Id = null!;
    }

    private QuestDefinition(
        QuestDefinitionId id,
        QuestType questType,
        QuestCadence cadence,
        int goal,
        int rewardAmount,
        QuestType? prerequisiteQuestType)
    {
        CheckRule(new QuestGoalMustBePositiveRule(goal));
        CheckRule(new QuestRewardAmountMustBePositiveRule(rewardAmount));

        Id = id;
        _questType = questType;
        _cadence = cadence;
        _goal = goal;
        _rewardAmount = rewardAmount;
        _prerequisiteQuestType = prerequisiteQuestType;
        _isActive = true;

        AddDomainEvent(new QuestDefinitionCreatedDomainEvent(
            id.Value,
            questType.ToString(),
            cadence.ToString(),
            goal,
            rewardAmount,
            prerequisiteQuestType?.ToString()));
    }

    public static QuestDefinition Create(
        QuestType questType,
        QuestCadence cadence,
        int goal,
        int rewardAmount,
        QuestType? prerequisiteQuestType)
    {
        return new QuestDefinition(
            new QuestDefinitionId(Guid.NewGuid()),
            questType,
            cadence,
            goal,
            rewardAmount,
            prerequisiteQuestType);
    }

    /// <summary>
    /// Update tunable fields. QuestType + Cadence are not mutable —
    /// changing them would re-key existing PlayerQuest history. To swap
    /// cadence, deactivate the definition and create a new one.
    /// </summary>
    public void Update(int goal, int rewardAmount, QuestType? prerequisiteQuestType)
    {
        CheckRule(new QuestGoalMustBePositiveRule(goal));
        CheckRule(new QuestRewardAmountMustBePositiveRule(rewardAmount));

        _goal = goal;
        _rewardAmount = rewardAmount;
        _prerequisiteQuestType = prerequisiteQuestType;

        AddDomainEvent(new QuestDefinitionUpdatedDomainEvent(
            Id.Value,
            goal,
            rewardAmount,
            prerequisiteQuestType?.ToString()));
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
