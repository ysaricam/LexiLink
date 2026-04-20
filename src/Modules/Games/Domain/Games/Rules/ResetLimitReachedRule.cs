using LexiLink.BuildingBlocks.Domain;

namespace LexiLink.Modules.Games.Domain.Games.Rules;

public sealed class ResetLimitReachedRule : IBusinessRule
{
    private readonly int _remainingResets;

    public ResetLimitReachedRule(int remainingResets)
    {
        _remainingResets = remainingResets;
    }

    public bool IsBroken() => _remainingResets <= 0;

    public string Message => "Başlangıca dönme hakkınız kalmadı (Maksimum 1 hak).";
}
