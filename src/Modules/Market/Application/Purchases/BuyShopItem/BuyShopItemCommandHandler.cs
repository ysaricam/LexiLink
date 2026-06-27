using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Domain;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Energy.Application.Configuration.CrossModule;
using LexiLink.Modules.Hint.Application.Configuration.CrossModule;
using LexiLink.Modules.Market.Application.Configuration.Commands;
using LexiLink.Modules.Market.Domain;
using LexiLink.Modules.Market.Domain.Rules;
using LexiLink.Modules.Reset.Application.Configuration.CrossModule;
using LexiLink.Modules.Undo.Application.Configuration.CrossModule;

namespace LexiLink.Modules.Market.Application.Purchases.BuyShopItem;

internal sealed class BuyShopItemCommandHandler
    : ICommandHandler<BuyShopItemCommand, BuyShopItemResultDto>
{
    private readonly IShopItemRepository _shopItemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IDiamondGuard _diamondGuard;
    private readonly IDiamondGrant _diamondGrant;
    private readonly IEnergyGrant _energyGrant;
    private readonly IHintGrant _hintGrant;
    private readonly IUndoGrant _undoGrant;
    private readonly IResetGrant _resetGrant;
    private readonly IClock _clock;

    internal BuyShopItemCommandHandler(
        IShopItemRepository shopItemRepository,
        ICategoryRepository categoryRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IDiamondGuard diamondGuard,
        IDiamondGrant diamondGrant,
        IEnergyGrant energyGrant,
        IHintGrant hintGrant,
        IUndoGrant undoGrant,
        IResetGrant resetGrant,
        IClock clock)
    {
        _shopItemRepository = shopItemRepository;
        _categoryRepository = categoryRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _diamondGuard = diamondGuard;
        _diamondGrant = diamondGrant;
        _energyGrant = energyGrant;
        _hintGrant = hintGrant;
        _undoGrant = undoGrant;
        _resetGrant = resetGrant;
        _clock = clock;
    }

    public async Task<BuyShopItemResultDto> Handle(
        BuyShopItemCommand request,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = request.IdempotencyKey.Trim();
        var existing = await _purchaseOrderRepository.GetByPlayerAndIdempotencyKeyAsync(
            request.PlayerId,
            idempotencyKey,
            cancellationToken);
        if (existing is not null)
        {
            return ToResult(existing, isReplay: true);
        }

        var shopItemId = new ShopItemId(request.ShopItemId);
        var shopItem = await _shopItemRepository.GetByIdAsync(shopItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(ShopItem), request.ShopItemId);

        var category = await _categoryRepository.GetByIdAsync(shopItem.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), shopItem.CategoryId.Value);

        var now = _clock.UtcNow;
        CheckRule(new ShopItemMustBeActiveRule(shopItem));
        CheckRule(new CategoryMustBeVisibleNowRule(category, now));
        CheckRule(new ShopItemMustHaveStockRemainingRule(shopItem));
        CheckRule(new DiamondSkusNotPurchasableInV1Rule(shopItem.ItemType));

        var (limitFrom, limitTo) = ResolveLimitWindow(shopItem, now);
        var purchaseCount = await _purchaseOrderRepository.CountByPlayerAndShopItemAsync(
            request.PlayerId,
            shopItemId,
            limitFrom,
            limitTo,
            cancellationToken);
        CheckRule(new PlayerMustNotExceedShopItemLimitRule(shopItem.PerPlayerLimit, purchaseCount));

        var effectivePrice = shopItem.EffectivePriceAt(now);

        if (shopItem.ItemType == ItemType.Energy)
        {
            await _energyGrant.EnsureCanAcceptAsync(
                request.PlayerId,
                shopItem.Quantity,
                cancellationToken);
        }

        await _diamondGuard.EnsureDiamondAvailableAsync(
            request.PlayerId,
            effectivePrice,
            cancellationToken);

        try
        {
            await GrantTargetInventoryAsync(
                request.PlayerId,
                shopItem.ItemType,
                shopItem.Quantity,
                cancellationToken);
        }
        catch
        {
            await _diamondGrant.GrantAsync(
                request.PlayerId,
                effectivePrice,
                cancellationToken);
            throw;
        }

        shopItem.RecordPurchase();
        var order = PurchaseOrder.Create(
            request.PlayerId,
            shopItemId,
            shopItem.ItemType,
            shopItem.Quantity,
            effectivePrice,
            now,
            idempotencyKey);
        await _purchaseOrderRepository.AddAsync(order, cancellationToken);

        return ToResult(order, isReplay: false);
    }

    private Task GrantTargetInventoryAsync(
        Guid playerId,
        ItemType itemType,
        int quantity,
        CancellationToken cancellationToken) =>
        itemType switch
        {
            ItemType.Energy => _energyGrant.GrantAsync(playerId, quantity, cancellationToken),
            ItemType.Hint => _hintGrant.GrantAsync(playerId, quantity, cancellationToken),
            ItemType.Undo => _undoGrant.GrantAsync(playerId, quantity, cancellationToken),
            ItemType.Reset => _resetGrant.GrantAsync(playerId, quantity, cancellationToken),
            ItemType.Diamond => throw new BusinessRuleValidationException(
                new DiamondSkusNotPurchasableInV1Rule(itemType)),
            _ => throw new InvalidOperationException($"Unsupported market item type '{itemType}'.")
        };

    private static (DateTime? From, DateTime? To) ResolveLimitWindow(ShopItem shopItem, DateTime now)
    {
        if (shopItem.PerPlayerLimit is null)
        {
            return (null, null);
        }

        return shopItem.PerPlayerLimitWindow switch
        {
            PerPlayerLimitWindow.Lifetime => (null, null),
            PerPlayerLimitWindow.Daily => (now.Date, now.Date.AddDays(1)),
            PerPlayerLimitWindow.PerPromo when shopItem.Promotion is not null
                && shopItem.Promotion.IsOpenAt(now) => (shopItem.Promotion.StartsAt, shopItem.Promotion.EndsAt),
            PerPlayerLimitWindow.PerPromo => (null, null),
            _ => (null, null)
        };
    }

    private static BuyShopItemResultDto ToResult(PurchaseOrder order, bool isReplay) =>
        new(
            order.Id.Value,
            order.ShopItemId.Value,
            order.ItemType.ToString(),
            order.Quantity,
            order.DiamondsPaid,
            order.PurchasedAt,
            isReplay);

    private static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}
