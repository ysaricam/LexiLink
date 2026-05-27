using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class ShopItemMustBeActiveRule : IBusinessRule
{
    private readonly ShopItem _shopItem;

    internal ShopItemMustBeActiveRule(ShopItem shopItem)
    {
        _shopItem = shopItem;
    }

    public bool IsBroken() => !_shopItem.IsActive;

    public string Message => "Shop item must be active.";
}
