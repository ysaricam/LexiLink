using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.ShopItems.UpdateShopItem;

internal sealed class UpdateShopItemCommandHandler : ICommandHandler<UpdateShopItemCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IShopItemRepository _shopItemRepository;

    internal UpdateShopItemCommandHandler(
        ICategoryRepository categoryRepository,
        IShopItemRepository shopItemRepository)
    {
        _categoryRepository = categoryRepository;
        _shopItemRepository = shopItemRepository;
    }

    public async Task Handle(UpdateShopItemCommand request, CancellationToken cancellationToken)
    {
        _ = await _categoryRepository.GetByIdAsync(new CategoryId(request.CategoryId), cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var shopItem = await _shopItemRepository.GetByIdAsync(
            new ShopItemId(request.ShopItemId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(ShopItem), request.ShopItemId);

        var promotion = request.PromoPrice is null
            ? null
            : Promotion.Create(
                request.PromoPrice.Value,
                request.Price,
                request.PromotionStartsAt!.Value,
                request.PromotionEndsAt!.Value);

        shopItem.Update(
            new CategoryId(request.CategoryId),
            request.ItemType,
            request.Quantity,
            request.Price,
            promotion,
            request.MaxStock,
            request.PerPlayerLimit,
            request.PerPlayerLimitWindow);
    }
}
