using LexiLink.BuildingBlocks.Domain;
using LexiLink.Modules.Games.Domain.GameLinks;
using LexiLink.Modules.Games.Domain.Score;
using LexiLink.Modules.Games.Domain.Games.Rules;
using LexiLink.Modules.Games.Domain.Categories;

namespace LexiLink.Modules.Games.Domain.Games;

public class Game : Entity, IAggregateRoot
{
    public GameId Id { get; private set; }
    public GameCategoryId CategoryId { get; private set; }
    public LinkId StartLinkId { get; }
    public int CurrentStep { get; private set; } = 0;
    public GameState State { get; private set; } = GameState.NotStarted;
    public ScoreValue Score { get; private set; } = ScoreValue.Zero;

    private LinkId? _targetLinkId;
    private LinkId _currentLinkId;
    private readonly List<LinkId> _history = new();
    private IReadOnlyList<LinkId> _targetPathIds = new List<LinkId>();

    private int _remainingUndos = 3;
    private int _remainingResets = 1;
    private int _remainingHints = 2;
    private int _consecutiveCorrectSteps = 0;

    private readonly ITargetLinkResolver _resolver;
    private readonly IScoreCalculator _scoreCalculator;
    private readonly int _targetDepth;
    private readonly int _maxSteps;

    private Game(
        GameCategoryId categoryId,
        LinkId startLinkId, 
        ITargetLinkResolver resolver, 
        IScoreCalculator scoreCalculator, 
        int targetDepth = 3, 
        int maxSteps = 10)
    {
        CheckRule(new GameStartLinkCannotBeNullRule(startLinkId));

        Id = new GameId(Guid.NewGuid());
        CategoryId = categoryId;
        StartLinkId = startLinkId!;
        _currentLinkId = startLinkId!;
        _targetDepth = targetDepth;
        _maxSteps = maxSteps;
        _resolver = resolver;
        _scoreCalculator = scoreCalculator;
    }

    internal static Game Create(
        GameCategoryId categoryId,
        LinkId startLinkId, 
        ITargetLinkResolver resolver, 
        IScoreCalculator scoreCalculator, 
        int targetDepth = 3, 
        int maxSteps = 10)
    {
        return new Game(categoryId, startLinkId, resolver, scoreCalculator, targetDepth, maxSteps);
    }

    public void Start()
    {
        CheckRule(new GameMustBeInSpecificStateRule(State, GameState.NotStarted));

        var resolution = _resolver.Resolve(StartLinkId, _targetDepth);
        
        _targetLinkId = resolution.TargetId;
        _targetPathIds = resolution.PathIds;
        
        _currentLinkId = StartLinkId;
        _history.Clear();
        _remainingUndos = 3;
        _remainingResets = 1;
        _remainingHints = 2;
        _consecutiveCorrectSteps = 0;
        CurrentStep = 0;
        Score = ScoreValue.Zero;
        State = GameState.InProgress;
    }

    public MoveResult MakeStep(LinkId nextLinkId, IReadOnlyList<LinkId> currentAvailableSubLinkIds)
    {
        CheckRule(new GameMustBeInSpecificStateRule(State, GameState.InProgress));
        CheckRule(new NextStepMustBeSubLinkOfCurrentRule(nextLinkId, currentAvailableSubLinkIds));

        _history.Add(_currentLinkId);
        
        // Kombo kontrolü: Eğer seçilen link hedef yoldaki sıradaki link ise kombo artar, değilse sıfırlanır.
        if (_targetPathIds.Count > CurrentStep && _targetPathIds[CurrentStep].Equals(nextLinkId))
        {
            _consecutiveCorrectSteps++;
        }
        else
        {
            _consecutiveCorrectSteps = 0;
        }

        _currentLinkId = nextLinkId;
        CurrentStep++;

        if(_currentLinkId == _targetLinkId)
        {
            State = GameState.Completed;
            Score = _scoreCalculator.Calculate(_targetDepth, CurrentStep, _maxSteps, _consecutiveCorrectSteps);
            return MoveResult.Completed;
        }
        else if(CurrentStep >= _maxSteps)
        {
            State = GameState.Failed;
            return MoveResult.Failed;
        }
        return MoveResult.Continue;
    }

    public HintResult GetHint()
    {
        CheckRule(new GameMustBeInSpecificStateRule(State, GameState.InProgress));
        CheckRule(new HintLimitReachedRule(_remainingHints));

        _remainingHints--;

        // 1. Durum: Oyuncu şu an doğru yolda mı?
        int currentIndexInPath = -1;
        for (int i = 0; i < _targetPathIds.Count; i++)
        {
            if (_targetPathIds[i].Equals(_currentLinkId))
            {
                currentIndexInPath = i;
                break;
            }
        }

        // Eğer başlangıç noktasındaysak ilk adımı önerelim
        if (_currentLinkId.Equals(StartLinkId))
        {
            return new HintResult("Doğru yoldasınız. İlk adım olarak bu kelimeyi seçebilirsiniz.", _targetPathIds[0], HintStatus.OnCorrectPath);
        }

        // Eğer doğru yoldaysak ve henüz sona gelmediysek
        if (currentIndexInPath != -1 && currentIndexInPath < _targetPathIds.Count - 1)
        {
            return new HintResult("Doğru yoldasınız. Bir sonraki adımınız bu olmalı.", _targetPathIds[currentIndexInPath + 1], HintStatus.OnCorrectPath);
        }

        // 2. Durum: Oyuncu yanlış yolda.
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            var historicLinkId = _history[i];
            
            int pathIndex = -1;
            for (int j = 0; j < _targetPathIds.Count; j++)
            {
                if (_targetPathIds[j].Equals(historicLinkId))
                {
                    pathIndex = j;
                    break;
                }
            }

            if (pathIndex != -1 || historicLinkId.Equals(StartLinkId))
            {
                _currentLinkId = historicLinkId;
                _history.RemoveRange(i, _history.Count - i);
                CurrentStep = _history.Count;

                var nextCorrectStepId = (pathIndex != -1 && pathIndex < _targetPathIds.Count - 1) 
                    ? _targetPathIds[pathIndex + 1] 
                    : (pathIndex == -1 ? _targetPathIds[0] : _targetLinkId!);

                return new HintResult("Yoldan sapmıştınız, sizi en son doğru yaptığınız adıma geri döndürdüm.", nextCorrectStepId, HintStatus.RedirectedToSafety);
            }
        }

        ResetToStart();
        return new HintResult("Yoldan çok uzaklaşmıştınız, sizi başlangıca geri döndürdüm.", _targetPathIds[0], HintStatus.RedirectedToSafety);
    }

    public void UndoMove()
    {
        CheckRule(new GameMustBeInSpecificStateRule(State, GameState.InProgress));
        CheckRule(new GameHasHistoryToUndoRule(_history.Count));
        CheckRule(new UndoLimitReachedRule(_remainingUndos));

        var lastIndex = _history.Count - 1;
        _currentLinkId = _history[lastIndex];
        _history.RemoveAt(lastIndex);
        
        _remainingUndos--;
        CurrentStep--;

        if (_consecutiveCorrectSteps > 0)
        {
            _consecutiveCorrectSteps--;
        }
    }

    public void ResetToStart()
    {
        CheckRule(new GameMustBeInSpecificStateRule(State, GameState.InProgress));
        CheckRule(new ResetLimitReachedRule(_remainingResets));
        
        _currentLinkId = StartLinkId;
        _history.Clear();
        _remainingResets--;
        _consecutiveCorrectSteps = 0;
        CurrentStep = 0;
    }

    public void Fail()
    {
        CheckRule(new GameMustBeInSpecificStateRule(State, GameState.InProgress));
        State = GameState.Failed;
    }

    public void Timeout()
    {
        CheckRule(new GameMustBeInSpecificStateRule(State, GameState.InProgress));
        State = GameState.TimedOut;
    }
}

public enum MoveResult
{
    Continue,
    Completed,
    Failed
}
