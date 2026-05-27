using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class ShopItemMustHaveStockRemainingRule : IBusinessRule
{
    private readonly ShopItem _shopItem;

    internal ShopItemMustHaveStockRemainingRule(ShopItem shopItem)
    {
        _shopItem = shopItem;
    }

    public bool IsBroken() => !_shopItem.HasStockRemaining();

    public string Message => "Shop item must have stock remaining.";
}
