using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public class GameMustNotBeFinishedRule : IBusinessRule
{
    private readonly GameState _gameState;

    public GameMustNotBeFinishedRule(GameState gameState)
    {
        _gameState = gameState;
    }

    public bool IsBroken() => _gameState == GameState.Completed || _gameState == GameState.Failed || _gameState == GameState.Abandoned;

    public string Message => "Game is already finished.";
}
