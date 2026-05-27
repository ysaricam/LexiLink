using LexiLink.Common.Domain;

namespace LexiLink.Modules.Market.Domain.Rules;

internal sealed class DiamondSkusNotPurchasableInV1Rule : IBusinessRule
{
    private readonly ItemType _itemType;

    internal DiamondSkusNotPurchasableInV1Rule(ItemType itemType)
    {
        _itemType = itemType;
    }

    public bool IsBroken() => _itemType == ItemType.Diamond;

    public string Message => "Diamond SKUs are reserved for future IAP and are not purchasable with Diamond in v1.";
}
