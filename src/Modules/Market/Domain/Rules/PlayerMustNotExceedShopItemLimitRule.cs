using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class PlayerMustNotExceedShopItemLimitRule : IBusinessRule
{
    private readonly int? _limit;
    private readonly int _currentPurchaseCount;

    internal PlayerMustNotExceedShopItemLimitRule(int? limit, int currentPurchaseCount)
    {
        _limit = limit;
        _currentPurchaseCount = currentPurchaseCount;
    }

    public bool IsBroken() => _limit is not null && _currentPurchaseCount >= _limit.Value;

    public string Message => "Player shop item limit has been reached.";
}
