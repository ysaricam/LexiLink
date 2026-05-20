using LexiLink.Common.Domain;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Events;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests;

public class PlayerQuest : Entity, IAggregateRoot
{
    public PlayerQuestId Id { get; private set; }

    private Guid _playerId;
    private QuestType _questType;
    private int _progress;
    private int _goal;
    private int _rewardAmount;
    private QuestState _state;
    private DateTime _issuedAt;
    private DateTime? _completedAt;
    private DateTime? _claimedAt;
    private DateTime? _expiresAt;

    public Guid PlayerId => _playerId;
    public QuestType QuestType => _questType;
    public int Progress => _progress;
    public int Goal => _goal;
    public int RewardAmount => _rewardAmount;
    public QuestState State => _state;
    public DateTime IssuedAt => _issuedAt;
    public DateTime? CompletedAt => _completedAt;
    public DateTime? ClaimedAt => _claimedAt;
    public DateTime? ExpiresAt => _expiresAt;

    private PlayerQuest()
    {
        Id = null!;
    }

    private PlayerQuest(
        PlayerQuestId id,
        Guid playerId,
        QuestType questType,
        int goal,
        int rewardAmount,
        DateTime issuedAt,
        DateTime? expiresAt)
    {
        CheckRule(new QuestGoalMustBePositiveRule(goal));
        CheckRule(new QuestRewardAmountMustBePositiveRule(rewardAmount));

        Id = id;
        _playerId = playerId;
        _questType = questType;
        _goal = goal;
        _rewardAmount = rewardAmount;
        _progress = 0;
        _state = QuestState.Active;
        _issuedAt = issuedAt;
        _expiresAt = expiresAt;

        AddDomainEvent(new PlayerQuestIssuedDomainEvent(Id, _playerId, _questType));
    }

    internal static PlayerQuest IssueFor(
        Guid playerId,
        QuestType questType,
        int goal,
        int rewardAmount,
        DateTime issuedAt,
        DateTime? expiresAt = null)
    {
        return new PlayerQuest(
            new PlayerQuestId(Guid.NewGuid()),
            playerId,
            questType,
            goal,
            rewardAmount,
            issuedAt,
            expiresAt);
    }

    internal void RecordProgress(int delta, DateTime now)
    {
        CheckRule(new QuestProgressDeltaMustBePositiveRule(delta));

        ExpireIfPast(now);

        CheckRule(new QuestMustBeActiveToProgressRule(_state));

        // Clamp so a late integration event cannot push progress beyond goal.
        var clampedDelta = Math.Min(delta, _goal - _progress);
        if (clampedDelta <= 0)
        {
            return;
        }

        _progress += clampedDelta;

        if (_progress >= _goal)
        {
            _state = QuestState.ReadyToClaim;
            _completedAt = now;
            AddDomainEvent(new PlayerQuestCompletedDomainEvent(Id, _playerId, _questType));
        }
    }

    internal void Claim(DateTime now)
    {
        ExpireIfPast(now);

        CheckRule(new QuestMustBeReadyToBeClaimedRule(_state));

        _state = QuestState.Claimed;
        _claimedAt = now;

        AddDomainEvent(new PlayerQuestClaimedDomainEvent(Id, _playerId, _questType, _rewardAmount));
    }

    internal void ExpireIfPast(DateTime now)
    {
        if (_state != QuestState.Active && _state != QuestState.ReadyToClaim)
        {
            return;
        }

        if (_expiresAt is null)
        {
            return;
        }

        if (now < _expiresAt.Value)
        {
            return;
        }

        _state = QuestState.Expired;
    }

    /// <summary>
    /// Admin-only force-reset: clears progress, re-arms the quest as
    /// Active, drops completed/claimed timestamps, and refreshes the
    /// expiry. The caller computes <paramref name="newExpiresAt"/>
    /// from the current definition's cadence (null for OneTime, next
    /// UTC midnight for Daily). Support tooling only — the regular
    /// issuance / progress / claim flow is unaffected by this method
    /// existing.
    /// </summary>
    public void AdminReset(DateTime now, DateTime? newExpiresAt)
    {
        _progress = 0;
        _state = QuestState.Active;
        _completedAt = null;
        _claimedAt = null;
        _issuedAt = now;
        _expiresAt = newExpiresAt;

        AddDomainEvent(new PlayerQuestAdminResetDomainEvent(Id, _playerId, _questType));
    }
}
