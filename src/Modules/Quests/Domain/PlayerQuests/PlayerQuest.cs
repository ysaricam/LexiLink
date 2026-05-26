using LexiLink.Common.Domain;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

/// <summary>
/// A player's instance of a <see cref="QuestDefinition"/>. Post Sprint Q1
/// the row stores only what cannot be derived from Stats: the snapshot
/// of the player's counter at issuance, the claim transition timestamp,
/// and (for daily quests) the expiry deadline. Progress and
/// ReadyToClaim are recomputed at every read from
/// <c>counter - ProgressBaselineSnapshot</c>.
/// </summary>
public class PlayerQuest : Entity, IAggregateRoot
{
    public PlayerQuestId Id { get; private set; }

    private Guid _playerId;
    private QuestDefinitionId _questDefinitionId = null!;
    private int _progressBaselineSnapshot;
    private QuestState _state;
    private DateTime _issuedAt;
    private DateTime? _claimedAt;
    private DateTime? _expiresAt;

    public Guid PlayerId => _playerId;
    public QuestDefinitionId QuestDefinitionId => _questDefinitionId;
    public int ProgressBaselineSnapshot => _progressBaselineSnapshot;
    public QuestState State => _state;
    public DateTime IssuedAt => _issuedAt;
    public DateTime? ClaimedAt => _claimedAt;
    public DateTime? ExpiresAt => _expiresAt;

    private PlayerQuest()
    {
        Id = null!;
    }

    private PlayerQuest(
        PlayerQuestId id,
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        int progressBaselineSnapshot,
        DateTime issuedAt,
        DateTime? expiresAt)
    {
        Id = id;
        _playerId = playerId;
        _questDefinitionId = questDefinitionId;
        _progressBaselineSnapshot = Math.Max(0, progressBaselineSnapshot);
        _state = QuestState.Active;
        _issuedAt = issuedAt;
        _expiresAt = expiresAt;

        AddDomainEvent(new PlayerQuestIssuedDomainEvent(Id, _playerId, _questDefinitionId));
    }

    internal static PlayerQuest IssueFor(
        Guid playerId,
        QuestDefinitionId questDefinitionId,
        int progressBaselineSnapshot,
        DateTime issuedAt,
        DateTime? expiresAt = null)
    {
        return new PlayerQuest(
            new PlayerQuestId(Guid.NewGuid()),
            playerId,
            questDefinitionId,
            progressBaselineSnapshot,
            issuedAt,
            expiresAt);
    }

    /// <summary>
    /// Transition Active → Claimed. The caller computes
    /// <paramref name="isReadyToClaim"/> from the current Stats counter
    /// (<c>counter - <see cref="ProgressBaselineSnapshot"/> &gt;=
    /// QuestDefinition.Threshold</c>) and the caller has also verified
    /// the row is not already past <see cref="ExpiresAt"/>. Reward
    /// values live on <see cref="QuestDefinition"/> and are carried
    /// into the claimed domain event by the handler so Energy, Hint,
    /// Undo, and Reset can each grant their portion event-driven.
    /// </summary>
    internal void Claim(
        DateTime now,
        bool isReadyToClaim,
        int energyReward,
        int hintReward,
        int undoReward,
        int resetReward)
    {
        CheckRule(new QuestMustBeReadyToBeClaimedRule(_state, isReadyToClaim));

        _state = QuestState.Claimed;
        _claimedAt = now;

        AddDomainEvent(new PlayerQuestClaimedDomainEvent(
            Id,
            _playerId,
            _questDefinitionId,
            energyReward,
            hintReward,
            undoReward,
            resetReward));
    }
}
