using LexiLink.Common.Application.Exceptions;
using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.Modules.Market.Application.Admin.ShopItems.CreateShopItem;

internal sealed class CreateShopItemCommandHandler : ICommandHandler<CreateShopItemCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IShopItemRepository _shopItemRepository;

    internal CreateShopItemCommandHandler(
        ICategoryRepository categoryRepository,
        IShopItemRepository shopItemRepository)
    {
        _categoryRepository = categoryRepository;
        _shopItemRepository = shopItemRepository;
    }

    public async Task<Guid> Handle(CreateShopItemCommand request, CancellationToken cancellationToken)
    {
        _ = await _categoryRepository.GetByIdAsync(new CategoryId(request.CategoryId), cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var promotion = request.PromoPrice is null
            ? null
            : Promotion.Create(
                request.PromoPrice.Value,
                request.Price,
                request.PromotionStartsAt!.Value,
                request.PromotionEndsAt!.Value);

        var shopItem = ShopItem.Create(
            new CategoryId(request.CategoryId),
            request.ItemType,
            request.Quantity,
            request.Price,
            promotion,
            request.MaxStock,
            request.PerPlayerLimit,
            request.PerPlayerLimitWindow);

        await _shopItemRepository.AddAsync(shopItem, cancellationToken);
        return shopItem.Id.Value;
    }
}
