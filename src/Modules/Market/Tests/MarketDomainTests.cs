using LexiLink.Modules.Market.Domain;
using LexiLink.Modules.Market.Domain.Events;
using LexiLink.Modules.Market.Domain.Rules;
using LexiLink.Modules.Market.Tests.SeedWork;

namespace LexiLink.Modules.Market.Tests;

[TestFixture]
public class MarketDomainTests : TestBase
{
    [Test]
    public void Category_Create_Should_PublishCategoryCreated()
    {
        var category = Category.Create("Boosts", 10, "boost", null);

        category.Name.Should().Be("Boosts");
        category.IsActive.Should().BeTrue();
        AssertPublishedDomainEvent<MarketCategoryCreatedDomainEvent>(category);
    }

    [Test]
    public void VisibilityWindow_Should_Reject_InvertedWindow()
    {
        AssertBrokenRule<WindowMustBeOrderedRule>(() =>
            VisibilityWindow.Create(DateTime.UtcNow.AddHours(1), DateTime.UtcNow));
    }

    [Test]
    public void Promotion_Should_Reject_PromoPriceNotLowerThanPrice()
    {
        AssertBrokenRule<PromotionPriceMustBeLessThanPriceRule>(() =>
            Promotion.Create(100, 100, DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));
    }

    [Test]
    public void ShopItem_RecordPurchase_Should_IncrementSoldCount_AndVersion()
    {
        var shopItem = ShopItem.Create(
            new CategoryId(Guid.NewGuid()),
            ItemType.Energy,
            quantity: 50,
            price: 10,
            promotion: null,
            maxStock: 5,
            perPlayerLimit: null,
            PerPlayerLimitWindow.Lifetime);

        shopItem.RecordPurchase();

        shopItem.SoldCount.Should().Be(1);
        shopItem.Version.Should().Be(1);
    }

    [Test]
    public void ShopItem_Create_Should_Reject_NonPositiveQuantity()
    {
        AssertBrokenRule<PositiveAmountRule>(() => ShopItem.Create(
            new CategoryId(Guid.NewGuid()),
            ItemType.Hint,
            quantity: 0,
            price: 10,
            promotion: null,
            maxStock: null,
            perPlayerLimit: null,
            PerPlayerLimitWindow.Lifetime));
    }

    [Test]
    public void PurchaseOrder_Create_Should_PublishOrderCreated()
    {
        var shopItemId = new ShopItemId(Guid.NewGuid());

        var order = PurchaseOrder.Create(
            Guid.NewGuid(),
            shopItemId,
            ItemType.Diamond,
            quantity: 1,
            diamondsPaid: 10,
            DateTime.UtcNow,
            "purchase-1");

        order.ShopItemId.Should().Be(shopItemId);
        AssertPublishedDomainEvent<PurchaseOrderCreatedDomainEvent>(order);
    }
}
