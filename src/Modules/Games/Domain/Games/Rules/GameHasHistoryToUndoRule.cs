using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public sealed class GameHasHistoryToUndoRule : IBusinessRule
{
    private readonly int _historyCount;

    public GameHasHistoryToUndoRule(int historyCount)
    {
        _historyCount = historyCount;
    }

    public bool IsBroken() => _historyCount == 0;

    public string Message => "Geri alınacak hamle bulunamadı. Zaten başlangıç noktasındasınız.";
}
