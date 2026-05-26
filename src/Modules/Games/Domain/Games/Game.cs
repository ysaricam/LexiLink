using LexiLink.Common.Domain;
using LexiLink.Modules.Games.Domain.Games.Allowances;
using LexiLink.Modules.Games.Domain.Games.Events;
using LexiLink.Modules.Games.Domain.Games.Puzzles;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Domain.Links;
using LexiLink.Modules.Games.Domain.Services;

namespace LexiLink.Modules.Games.Domain.Games;

public class Game : Entity, IAggregateRoot
{
    private readonly List<GameHistoryStep> _history;
    private readonly Puzzle _puzzle;
    private Score? _score;

    public GameId Id { get; private set; }
    public Guid PlayerId { get; private set; }

    private LinkId _currentLinkId;

    private GameState _gameState;

    private StepBudget _stepBudget;

    private HintAllowance _hintAllowance;
    private int _undosUsed;
    private int _resetsUsed;

    public LinkId CurrentLinkId => _currentLinkId;
    public IReadOnlyCollection<LinkId> History => _history.Select(s => s.LinkId).ToList().AsReadOnly();
    public GameState State => _gameState;
    public Score? Score => _score;

    private Game()
    {
        _history = [];
    }

    private Game(
        Guid playerId,
        Puzzle puzzle,
        int maxSteps,
        int hints)
    {
        Id = new GameId(Guid.NewGuid());
        PlayerId = playerId;

        _puzzle = puzzle;

        _currentLinkId = puzzle.StartLinkId;

        _stepBudget = StepBudget.Of(maxSteps);

        _hintAllowance = HintAllowance.Of(hints);
        _undosUsed = 0;
        _resetsUsed = 0;

        _history = [];
        _gameState = GameState.Initial;

        AddDomainEvent(new GameCreatedDomainEvent(Id));
    }

    internal static Game Create(
        Guid playerId,
        Puzzle puzzle,
        int maxSteps,
        int hints)
    {
        return new Game(playerId, puzzle, maxSteps, hints);
    }

    public void Start()
    {
        CheckRule(new GameMustBeNotStartedRule(_gameState));

        _gameState = GameState.InProgress;
        AddDomainEvent(new GameStartedDomainEvent(Id));
    }

    public void MakeStep(
        LinkId nextLinkId,
        ILinkNeighborResolver neighborResolver,
        IScoreCalculator scoreCalculator)
    {
        CheckRule(new GameMustBeInProgressRule(_gameState));

        var validOutgoingLinkIds = neighborResolver.GetOutgoingLinkIds(_currentLinkId);
        CheckRule(new StepMustBeValidRule(nextLinkId, validOutgoingLinkIds));

        _currentLinkId = nextLinkId;
        _history.Add(new GameHistoryStep(_history.Count + 1, nextLinkId));
        _stepBudget = _stepBudget.Step();
        AddDomainEvent(new StepMadeDomainEvent(Id, nextLinkId));

        EvaluatePostStepTransitions(scoreCalculator);
    }

    private void EvaluatePostStepTransitions(IScoreCalculator scoreCalculator)
    {
        if (_currentLinkId == _puzzle.TargetLinkId)
        {
            Complete(scoreCalculator);
            return;
        }

        if (_stepBudget.IsExhausted)
        {
            Fail();
            return;
        }

        if (_stepBudget.IsAtLastWarning && _gameState != GameState.LastStepWarning)
        {
            _gameState = GameState.LastStepWarning;
            AddDomainEvent(new LastStepWarningIssuedDomainEvent(Id));
        }
    }

    /// <summary>
    /// True while the per-game free hint allowance still has at least
    /// one charge. The handler consults this to decide whether to call
    /// <see cref="UseHint"/> (free path) or fall through to the
    /// player's persistent hint inventory via the IHintGuard sync
    /// gateway.
    /// </summary>
    public bool HasFreeHintRemaining => _hintAllowance.Remaining > 0;

    public HintResult UseHint()
    {
        CheckRule(new GameMustBeInProgressRule(_gameState));

        _hintAllowance = _hintAllowance.Consume();
        var hintResult = _puzzle.RequestHint(_currentLinkId);

        AddDomainEvent(new HintUsedDomainEvent(Id, hintResult));

        return hintResult;
    }

    /// <summary>
    /// Same effect on the puzzle as <see cref="UseHint"/> but does
    /// NOT touch the per-game free allowance — the caller has already
    /// consumed one charge from the player's persistent
    /// PlayerHintInventory via <c>IHintGuard.EnsureHintAvailableAsync</c>.
    /// </summary>
    public HintResult UseHintWithExternalInventory()
    {
        CheckRule(new GameMustBeInProgressRule(_gameState));

        var hintResult = _puzzle.RequestHint(_currentLinkId);

        AddDomainEvent(new HintUsedDomainEvent(Id, hintResult));

        return hintResult;
    }

    public void Undo()
    {
        UseUndoWithExternalInventory();
    }

    public void UseUndoWithExternalInventory()
    {
        CheckRule(new GameMustBeInProgressRule(_gameState));
        CheckRule(new GameHistoryMustNotBeEmptyRule(_history));

        _history.RemoveAt(_history.Count - 1);
        _currentLinkId = _history.Count > 0 ? _history[^1].LinkId : _puzzle.StartLinkId;
        _stepBudget = _stepBudget.UndoStep();
        _undosUsed++;

        if (_gameState == GameState.LastStepWarning && _stepBudget.IsBelowLastWarning)
        {
            _gameState = GameState.InProgress;
        }

        AddDomainEvent(new UndoUsedDomainEvent(Id));
    }

    public void ResetToStart()
    {
        ResetWithExternalInventory();
    }

    public void ResetWithExternalInventory()
    {
        CheckRule(new GameMustBeInProgressRule(_gameState));
        CheckRule(new GameHistoryMustNotBeEmptyRule(_history));

        _currentLinkId = _puzzle.StartLinkId;
        _history.Clear();
        _stepBudget = _stepBudget.Reset();
        _gameState = GameState.InProgress;
        _resetsUsed++;

        AddDomainEvent(new ResetUsedDomainEvent(Id));
    }

    public void Abandon()
    {
        CheckRule(new GameMustNotBeFinishedRule(_gameState));

        _gameState = GameState.Abandoned;
        AddDomainEvent(new GameAbandonedDomainEvent(Id));
    }

    private void Complete(IScoreCalculator scoreCalculator)
    {
        _gameState = GameState.Completed;
        _score = scoreCalculator.Calculate(
            difficulty: _puzzle.Difficulty,
            targetDepth: _puzzle.Depth,
            stepsTaken: _history.Count,
            hintsUsed: _hintAllowance.Used,
            undosUsed: _undosUsed,
            resetsUsed: _resetsUsed);

        AddDomainEvent(new GameCompletedDomainEvent(Id, PlayerId, _puzzle.StartLinkId, _puzzle.TargetLinkId, _score));
    }

    private void Fail()
    {
        _gameState = GameState.Failed;
        AddDomainEvent(new GameFailedDomainEvent(Id));
    }
}
