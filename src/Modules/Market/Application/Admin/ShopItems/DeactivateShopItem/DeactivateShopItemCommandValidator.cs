using FluentValidation;

namespace LexiLink.Modules.Market.Application.Admin.ShopItems.DeactivateShopItem;

internal sealed class DeactivateShopItemCommandValidator : AbstractValidator<DeactivateShopItemCommand>
{
    public DeactivateShopItemCommandValidator()
    {
        RuleFor(x => x.ShopItemId).NotEmpty();
    }
}
