using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public class GameMustBeInProgressRule : IBusinessRule
{
    private readonly GameState _gameState;

    public GameMustBeInProgressRule(GameState gameState)
    {
        _gameState = gameState;
    }

    public bool IsBroken() => _gameState != GameState.InProgress && _gameState != GameState.LastStepWarning;

    public string Message => "Game is not in progress.";
}
