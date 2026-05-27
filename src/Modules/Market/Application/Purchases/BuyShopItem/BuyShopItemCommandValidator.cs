using FluentValidation;

namespace LexiLink.Modules.Market.Application.Purchases.BuyShopItem;

internal sealed class BuyShopItemCommandValidator : AbstractValidator<BuyShopItemCommand>
{
    public BuyShopItemCommandValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.ShopItemId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(128);
    }
}
