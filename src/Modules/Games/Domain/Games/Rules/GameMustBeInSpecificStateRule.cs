using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public sealed class GameMustBeInSpecificStateRule : IBusinessRule
{
    private readonly GameState _currentState;
    private readonly GameState _expectedState;

    public GameMustBeInSpecificStateRule(GameState currentState, GameState expectedState)
    {
        _currentState = currentState;
        _expectedState = expectedState;
    }

    public bool IsBroken() => _currentState != _expectedState;

    public string Message => $"Geçersiz işlem: Oyun {_expectedState} durumunda olmalıdır (Şu anki durum: {_currentState}).";
}
