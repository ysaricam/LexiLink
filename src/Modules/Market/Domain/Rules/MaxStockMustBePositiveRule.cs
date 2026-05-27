using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class MaxStockMustBePositiveRule : IBusinessRule
{
    private readonly int? _maxStock;

    internal MaxStockMustBePositiveRule(int? maxStock)
    {
        _maxStock = maxStock;
    }

    public bool IsBroken() => _maxStock is not null && _maxStock.Value <= 0;

    public string Message => "Max stock must be greater than zero when set.";
}
