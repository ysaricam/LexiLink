using LexiLink.Common.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public class GameHistoryMustNotBeEmptyRule : IBusinessRule
{
    private readonly IReadOnlyCollection<GameHistoryStep> _history;

    public GameHistoryMustNotBeEmptyRule(IReadOnlyCollection<GameHistoryStep> history)
    {
        _history = history;
    }

    public bool IsBroken() => _history.Count == 0;

    public string Message => "Game history is empty. Cannot undo.";
}
