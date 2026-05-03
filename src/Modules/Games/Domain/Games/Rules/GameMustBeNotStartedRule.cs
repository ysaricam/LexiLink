using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public class GameMustBeNotStartedRule : IBusinessRule
{
    private readonly GameState _gameState;

    public GameMustBeNotStartedRule(GameState gameState)
    {
        _gameState = gameState;
    }

    public bool IsBroken() => _gameState != GameState.Initial;

    public string Message => "Game has already been started.";
}
