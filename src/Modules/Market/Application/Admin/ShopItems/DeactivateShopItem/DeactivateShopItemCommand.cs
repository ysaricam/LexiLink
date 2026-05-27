using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Market.Application.Contracts;

namespace LexiLink.Modules.Market.Application.Admin.ShopItems.DeactivateShopItem;

public sealed class DeactivateShopItemCommand : CommandBase, IAdminCommand
{
    public Guid ShopItemId { get; }

    public DeactivateShopItemCommand(Guid shopItemId)
    {
        ShopItemId = shopItemId;
    }

    public string AuditTargetType => "Market.ShopItem";
    public string? AuditTargetId => ShopItemId.ToString();
}
