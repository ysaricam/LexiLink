using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public sealed class UndoLimitReachedRule : IBusinessRule
{
    private readonly int _remainingUndos;

    public UndoLimitReachedRule(int remainingUndos)
    {
        _remainingUndos = remainingUndos;
    }

    public bool IsBroken() => _remainingUndos <= 0;

    public string Message => "Geri alma hakkınız kalmadı (Maksimum 3 hak).";
}
