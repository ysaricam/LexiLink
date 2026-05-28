using LexiLink.Modules.Payments.Domain;
using LexiLink.Modules.Payments.Domain.Events;
using LexiLink.Modules.Payments.Domain.Rules;
using LexiLink.Modules.Payments.Tests.SeedWork;

namespace LexiLink.Modules.Payments.Tests;

[TestFixture]
public class PaymentsDomainTests : TestBase
{
    [Test]
    public void PaymentProduct_Create_Should_PublishProductCreated()
    {
        var product = PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10);

        product.DiamondAmount.Should().Be(100);
        product.Supports(PaymentPlatform.Apple).Should().BeTrue();
        AssertPublishedDomainEvent<PaymentProductCreatedDomainEvent>(product);
    }

    [Test]
    public void PaymentProduct_Create_Should_Reject_NoPlatformAvailability()
    {
        AssertBrokenRule<ProductMustSupportAtLeastOnePlatformRule>(() =>
            PaymentProduct.Create(
                StoreProductId.Of("diamond_100"),
                diamondAmount: 100,
                isAppleAvailable: false,
                isGoogleAvailable: false,
                sortOrder: 10));
    }

    [Test]
    public void PaymentProduct_Update_Should_PublishProductUpdated()
    {
        var product = PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10);

        product.ClearDomainEvents();
        product.Update(
            diamondAmount: 550,
            isAppleAvailable: false,
            isGoogleAvailable: true,
            sortOrder: 20);

        product.DiamondAmount.Should().Be(550);
        product.Supports(PaymentPlatform.Apple).Should().BeFalse();
        product.Supports(PaymentPlatform.Google).Should().BeTrue();
        product.SortOrder.Should().Be(20);
        AssertPublishedDomainEvent<PaymentProductUpdatedDomainEvent>(product);
    }

    [Test]
    public void PaymentProduct_Deactivate_Should_PublishProductDeactivated()
    {
        var product = PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10);

        product.ClearDomainEvents();
        product.Deactivate();

        product.IsActive.Should().BeFalse();
        AssertPublishedDomainEvent<PaymentProductDeactivatedDomainEvent>(product);
    }

    [Test]
    public void StoreProductId_Should_Reject_EmptyValue()
    {
        AssertBrokenRule<TextMustNotBeEmptyRule>(() => StoreProductId.Of(" "));
    }

    [Test]
    public void IapPurchase_CreateReceived_Should_Require_AppleTransactionId()
    {
        AssertBrokenRule<StoreProofMustMatchPlatformRule>(() =>
            IapPurchase.CreateReceived(
                Guid.NewGuid(),
                PaymentPlatform.Apple,
                PaymentEnvironment.Sandbox,
                StoreProductId.Of("diamond_100"),
                storeTransactionId: null,
                purchaseToken: null,
                orderId: null,
                clientRequestId: "request-1",
                diamondAmount: 100,
                IapPurchasePostProcessingAction.AppleClientFinishTransaction,
                DateTime.UtcNow));
    }

    [Test]
    public void IapPurchase_CreateReceived_Should_PublishPurchaseReceived()
    {
        var purchase = IapPurchase.CreateReceived(
            Guid.NewGuid(),
            PaymentPlatform.Google,
            PaymentEnvironment.Sandbox,
            StoreProductId.Of("diamond_100"),
            storeTransactionId: null,
            purchaseToken: PurchaseToken.Of("purchase-token"),
            orderId: "order-1",
            clientRequestId: "request-1",
            diamondAmount: 100,
            IapPurchasePostProcessingAction.GoogleConsume,
            DateTime.UtcNow);

        purchase.Status.Should().Be(IapPurchaseStatus.Received);
        AssertPublishedDomainEvent<IapPurchaseReceivedDomainEvent>(purchase);
    }

    [Test]
    public void IapPurchase_MarkVerifiedButGrantFailed_Should_KeepRecoverableStatus()
    {
        var purchase = IapPurchase.CreateReceived(
            Guid.NewGuid(),
            PaymentPlatform.Apple,
            PaymentEnvironment.Sandbox,
            StoreProductId.Of("diamond_100"),
            StoreTransactionId.Of("transaction-1"),
            purchaseToken: null,
            orderId: null,
            clientRequestId: null,
            diamondAmount: 100,
            IapPurchasePostProcessingAction.AppleClientFinishTransaction,
            DateTime.UtcNow);

        purchase.MarkVerified(DateTime.UtcNow);
        purchase.MarkVerifiedButGrantFailed("Diamond module unavailable");

        purchase.Status.Should().Be(IapPurchaseStatus.VerifiedButGrantFailed);
        purchase.FailureReason.Should().Be("Diamond module unavailable");
    }

    [Test]
    public void PaymentNotification_Receive_Should_PublishNotificationReceived()
    {
        var notification = PaymentNotification.Receive(
            PaymentPlatform.Apple,
            PaymentEnvironment.Sandbox,
            notificationId: "notification-1",
            notificationType: "CONSUMPTION_REQUEST",
            payloadJson: "{}",
            receivedAt: DateTime.UtcNow);

        notification.Status.Should().Be(PaymentNotificationStatus.Received);
        AssertPublishedDomainEvent<PaymentNotificationReceivedDomainEvent>(notification);
    }
}
