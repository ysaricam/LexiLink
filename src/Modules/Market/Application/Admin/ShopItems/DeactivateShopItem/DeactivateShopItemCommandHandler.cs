using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.ShopItems.DeactivateShopItem;

internal sealed class DeactivateShopItemCommandHandler : ICommandHandler<DeactivateShopItemCommand>
{
    private readonly IShopItemRepository _shopItemRepository;

    internal DeactivateShopItemCommandHandler(IShopItemRepository shopItemRepository)
    {
        _shopItemRepository = shopItemRepository;
    }

    public async Task Handle(DeactivateShopItemCommand request, CancellationToken cancellationToken)
    {
        var shopItem = await _shopItemRepository.GetByIdAsync(
            new ShopItemId(request.ShopItemId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(ShopItem), request.ShopItemId);

        shopItem.Deactivate();
    }
}
