using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class PromotionPriceMustBeLessThanPriceRule : IBusinessRule
{
    private readonly int _promoPrice;
    private readonly int _price;

    internal PromotionPriceMustBeLessThanPriceRule(int promoPrice, int price)
    {
        _promoPrice = promoPrice;
        _price = price;
    }

    public bool IsBroken() => _promoPrice <= 0 || _promoPrice >= _price;

    public string Message => "Promotion price must be positive and less than the base price.";
}
