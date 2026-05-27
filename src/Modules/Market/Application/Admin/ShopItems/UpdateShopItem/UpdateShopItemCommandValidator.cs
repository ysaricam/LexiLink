using FluentValidation;

namespace LexiLink.Modules.Market.Application.Admin.ShopItems.UpdateShopItem;

internal sealed class UpdateShopItemCommandValidator : AbstractValidator<UpdateShopItemCommand>
{
    public UpdateShopItemCommandValidator()
    {
        RuleFor(x => x.ShopItemId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.MaxStock).GreaterThan(0).When(x => x.MaxStock.HasValue);
        RuleFor(x => x.PerPlayerLimit).GreaterThan(0).When(x => x.PerPlayerLimit.HasValue);
        RuleFor(x => new { x.PromoPrice, x.PromotionStartsAt, x.PromotionEndsAt })
            .Must(x => x.PromoPrice.HasValue == x.PromotionStartsAt.HasValue
                       && x.PromoPrice.HasValue == x.PromotionEndsAt.HasValue)
            .WithMessage("Promotion price, start, and end must be provided together.");
    }
}
