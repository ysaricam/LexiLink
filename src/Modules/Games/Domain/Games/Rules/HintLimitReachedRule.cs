using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public sealed class HintLimitReachedRule : IBusinessRule
{
    private readonly int _remainingHints;

    public HintLimitReachedRule(int remainingHints)
    {
        _remainingHints = remainingHints;
    }

    public bool IsBroken() => _remainingHints <= 0;

    public string Message => "İpucu hakkınız kalmadı (Maksimum 2 hak).";
}
