using LexiLink.Common.Application.Time;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Payments.Application.Configuration.Notifications;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Application.IapPurchases.RetryIapPurchaseDelivery;
using LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;
using LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;
using LexiLink.Modules.Payments.Domain;
using LexiLink.Modules.Payments.Tests.Fakes;

namespace LexiLink.Modules.Payments.Tests;

[TestFixture]
public class VerifyIapPurchaseCommandTests
{
    [Test]
    public async Task VerifyIapPurchaseCommand_Should_GrantDiamond_WhenApplePurchaseIsVerified()
    {
        var playerId = Guid.NewGuid();
        var productRepository = new FakePaymentProductRepository();
        productRepository.Add(PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10));

        var appleVerifier = new FakeAppleIapVerifier();
        appleVerifier.AddVerified(
            transactionId: "apple-transaction-1",
            storeProductId: "diamond_100",
            accountToken: playerId.ToString());

        var purchaseRepository = new FakeIapPurchaseRepository();
        var diamondGrant = new FakeDiamondGrant();
        var handler = CreateHandler(productRepository, purchaseRepository, appleVerifier, diamondGrant);

        var result = await handler.Handle(
            new VerifyIapPurchaseCommand(
                playerId,
                PaymentPlatform.Apple,
                "diamond_100",
                "apple-transaction-1",
                purchaseToken: null,
                signedTransactionJws: "signed-jws",
                accountToken: playerId.ToString(),
                clientRequestId: "request-1"),
            CancellationToken.None);

        result.Status.Should().Be(IapPurchaseStatus.Granted.ToString());
        result.DiamondAmount.Should().Be(100);
        result.IsReplay.Should().BeFalse();
        diamondGrant.Grants.Should().ContainSingle().Which.Should().Be((playerId, 100));
        purchaseRepository.Purchases.Should().ContainSingle().Which.Status.Should().Be(IapPurchaseStatus.Granted);
    }

    [Test]
    public async Task VerifyIapPurchaseCommand_Should_ReturnReplay_WithoutSecondGrant()
    {
        var playerId = Guid.NewGuid();
        var productRepository = new FakePaymentProductRepository();
        productRepository.Add(PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10));

        var appleVerifier = new FakeAppleIapVerifier();
        appleVerifier.AddVerified(
            transactionId: "apple-transaction-1",
            storeProductId: "diamond_100",
            accountToken: playerId.ToString());

        var purchaseRepository = new FakeIapPurchaseRepository();
        var diamondGrant = new FakeDiamondGrant();
        var handler = CreateHandler(productRepository, purchaseRepository, appleVerifier, diamondGrant);
        var command = new VerifyIapPurchaseCommand(
            playerId,
            PaymentPlatform.Apple,
            "diamond_100",
            "apple-transaction-1",
            purchaseToken: null,
            signedTransactionJws: "signed-jws",
            accountToken: playerId.ToString(),
            clientRequestId: "request-1");

        var first = await handler.Handle(command, CancellationToken.None);
        var replay = await handler.Handle(command, CancellationToken.None);

        first.IsReplay.Should().BeFalse();
        replay.IsReplay.Should().BeTrue();
        replay.PaymentId.Should().Be(first.PaymentId);
        diamondGrant.Grants.Should().ContainSingle();
        purchaseRepository.Purchases.Should().ContainSingle();
    }

    [Test]
    public async Task VerifyIapPurchaseCommand_Should_RecordRecoverableStatus_WhenDiamondGrantFails()
    {
        var playerId = Guid.NewGuid();
        var productRepository = new FakePaymentProductRepository();
        productRepository.Add(PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10));

        var appleVerifier = new FakeAppleIapVerifier();
        appleVerifier.AddVerified(
            transactionId: "apple-transaction-1",
            storeProductId: "diamond_100",
            accountToken: playerId.ToString());

        var purchaseRepository = new FakeIapPurchaseRepository();
        var diamondGrant = new FakeDiamondGrant
        {
            ShouldFail = true
        };
        var handler = CreateHandler(productRepository, purchaseRepository, appleVerifier, diamondGrant);

        var result = await handler.Handle(
            new VerifyIapPurchaseCommand(
                playerId,
                PaymentPlatform.Apple,
                "diamond_100",
                "apple-transaction-1",
                purchaseToken: null,
                signedTransactionJws: "signed-jws",
                accountToken: playerId.ToString(),
                clientRequestId: null),
            CancellationToken.None);

        result.Status.Should().Be(IapPurchaseStatus.VerifiedButGrantFailed.ToString());
        purchaseRepository.Purchases.Should().ContainSingle().Which.Status.Should()
            .Be(IapPurchaseStatus.VerifiedButGrantFailed);
        diamondGrant.Grants.Should().ContainSingle().Which.Should().Be((playerId, 100));
    }

    [Test]
    public async Task VerifyIapPurchaseCommand_Should_ConsumeGooglePurchase_AfterGrant()
    {
        var playerId = Guid.NewGuid();
        var productRepository = new FakePaymentProductRepository();
        productRepository.Add(PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10));

        var googleVerifier = new FakeGooglePlayIapVerifier();
        googleVerifier.AddVerified(
            purchaseToken: "google-token-1",
            storeProductId: "diamond_100",
            accountToken: playerId.ToString());

        var googleProcessor = new FakeGooglePlayPurchaseProcessor();
        var handler = CreateHandler(
            productRepository,
            new FakeIapPurchaseRepository(),
            new FakeAppleIapVerifier(),
            new FakeDiamondGrant(),
            googleVerifier,
            googleProcessor);

        var result = await handler.Handle(
            new VerifyIapPurchaseCommand(
                playerId,
                PaymentPlatform.Google,
                "diamond_100",
                storeTransactionId: null,
                purchaseToken: "google-token-1",
                signedTransactionJws: null,
                accountToken: playerId.ToString(),
                clientRequestId: null),
            CancellationToken.None);

        result.Status.Should().Be(IapPurchaseStatus.Granted.ToString());
        result.PostProcessingAction.Should().Be(IapPurchasePostProcessingAction.GoogleConsume.ToString());
        result.PostProcessingStatus.Should().Be(IapPurchasePostProcessingStatus.Succeeded.ToString());
        result.CanFinishTransaction.Should().BeFalse();
        googleProcessor.ConsumedTokens.Should().ContainSingle().Which.Should().Be("google-token-1");
    }

    [Test]
    public async Task RetryIapPurchaseDeliveryCommand_Should_RetryFailedGooglePostProcessing()
    {
        var playerId = Guid.NewGuid();
        var productRepository = new FakePaymentProductRepository();
        productRepository.Add(PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10));

        var googleVerifier = new FakeGooglePlayIapVerifier();
        googleVerifier.AddVerified(
            purchaseToken: "google-token-1",
            storeProductId: "diamond_100",
            accountToken: playerId.ToString());

        var purchaseRepository = new FakeIapPurchaseRepository();
        var googleProcessor = new FakeGooglePlayPurchaseProcessor
        {
            FailConsume = true
        };
        var verifyHandler = CreateHandler(
            productRepository,
            purchaseRepository,
            new FakeAppleIapVerifier(),
            new FakeDiamondGrant(),
            googleVerifier,
            googleProcessor);

        var first = await verifyHandler.Handle(
            new VerifyIapPurchaseCommand(
                playerId,
                PaymentPlatform.Google,
                "diamond_100",
                storeTransactionId: null,
                purchaseToken: "google-token-1",
                signedTransactionJws: null,
                accountToken: playerId.ToString(),
                clientRequestId: null),
            CancellationToken.None);

        first.PostProcessingStatus.Should().Be(IapPurchasePostProcessingStatus.Failed.ToString());
        googleProcessor.FailConsume = false;
        var retryHandler = new RetryIapPurchaseDeliveryCommandHandler(
            purchaseRepository,
            new FakeDiamondGrant(),
            googleProcessor,
            new FixedClock(new DateTime(2026, 5, 28, 12, 1, 0, DateTimeKind.Utc)));

        var retry = await retryHandler.Handle(
            new RetryIapPurchaseDeliveryCommand(first.PaymentId),
            CancellationToken.None);

        retry.Status.Should().Be(IapPurchaseStatus.Granted.ToString());
        retry.PostProcessingStatus.Should().Be(IapPurchasePostProcessingStatus.Succeeded.ToString());
        googleProcessor.ConsumedTokens.Should().ContainSingle().Which.Should().Be("google-token-1");
    }

    [Test]
    public async Task RetryIapPurchaseDeliveryCommand_Should_GrantAndAllowAppleFinish_WhenDeliveryFailedBefore()
    {
        var playerId = Guid.NewGuid();
        var productRepository = new FakePaymentProductRepository();
        productRepository.Add(PaymentProduct.Create(
            StoreProductId.Of("diamond_100"),
            diamondAmount: 100,
            isAppleAvailable: true,
            isGoogleAvailable: true,
            sortOrder: 10));

        var appleVerifier = new FakeAppleIapVerifier();
        appleVerifier.AddVerified(
            transactionId: "apple-transaction-1",
            storeProductId: "diamond_100",
            accountToken: playerId.ToString());

        var purchaseRepository = new FakeIapPurchaseRepository();
        var failingGrant = new FakeDiamondGrant
        {
            ShouldFail = true
        };
        var verifyHandler = CreateHandler(
            productRepository,
            purchaseRepository,
            appleVerifier,
            failingGrant);

        var failed = await verifyHandler.Handle(
            new VerifyIapPurchaseCommand(
                playerId,
                PaymentPlatform.Apple,
                "diamond_100",
                "apple-transaction-1",
                purchaseToken: null,
                signedTransactionJws: "signed-jws",
                accountToken: playerId.ToString(),
                clientRequestId: null),
            CancellationToken.None);

        failed.Status.Should().Be(IapPurchaseStatus.VerifiedButGrantFailed.ToString());
        var successfulGrant = new FakeDiamondGrant();
        var retryHandler = new RetryIapPurchaseDeliveryCommandHandler(
            purchaseRepository,
            successfulGrant,
            new FakeGooglePlayPurchaseProcessor(),
            new FixedClock(new DateTime(2026, 5, 28, 12, 1, 0, DateTimeKind.Utc)));

        var retry = await retryHandler.Handle(
            new RetryIapPurchaseDeliveryCommand(failed.PaymentId),
            CancellationToken.None);

        retry.Status.Should().Be(IapPurchaseStatus.Granted.ToString());
        retry.PostProcessingStatus.Should().Be(IapPurchasePostProcessingStatus.Succeeded.ToString());
        retry.CanFinishTransaction.Should().BeTrue();
        successfulGrant.Grants.Should().ContainSingle().Which.Should().Be((playerId, 100));
    }

    [Test]
    public async Task ReceiveApplePaymentNotificationCommand_Should_MarkPurchaseRefunded()
    {
        var playerId = Guid.NewGuid();
        var purchaseRepository = new FakeIapPurchaseRepository();
        var purchase = IapPurchase.CreateReceived(
            playerId,
            PaymentPlatform.Apple,
            PaymentEnvironment.Sandbox,
            StoreProductId.Of("diamond_100"),
            StoreTransactionId.Of("apple-transaction-1"),
            purchaseToken: null,
            orderId: null,
            clientRequestId: null,
            diamondAmount: 100,
            IapPurchasePostProcessingAction.AppleClientFinishTransaction,
            DateTime.UtcNow);
        purchase.MarkVerified(DateTime.UtcNow);
        purchase.MarkGranted(DateTime.UtcNow);
        await purchaseRepository.AddAsync(purchase);

        var notificationRepository = new FakePaymentNotificationRepository();
        var verifier = new FakeAppleServerNotificationVerifier();
        verifier.Result = PaymentNotificationVerificationResult.Verified(
            PaymentPlatform.Apple,
            PaymentEnvironment.Sandbox,
            notificationId: "apple-notification-1",
            notificationType: "REFUND",
            payloadJson: "{\"type\":\"REFUND\"}",
            storeTransactionId: "apple-transaction-1",
            purchaseToken: null,
            PaymentNotificationPurchaseStatus.Refunded,
            reason: "Refunded by App Store.");
        var handler = new ReceiveApplePaymentNotificationCommandHandler(
            verifier,
            new ReceivePaymentNotificationProcessor(
                notificationRepository,
                purchaseRepository,
                new FixedClock(new DateTime(2026, 5, 28, 12, 2, 0, DateTimeKind.Utc))));

        var result = await handler.Handle(
            new ReceiveApplePaymentNotificationCommand("signed-payload"),
            CancellationToken.None);

        result.Status.Should().Be(PaymentNotificationStatus.Processed.ToString());
        result.IapPurchaseId.Should().Be(purchase.Id.Value);
        result.IapPurchaseStatus.Should().Be(IapPurchaseStatus.Refunded.ToString());
        purchase.Status.Should().Be(IapPurchaseStatus.Refunded);
        notificationRepository.Notifications.Should().ContainSingle();
    }

    [Test]
    public async Task ReceiveGooglePaymentNotificationCommand_Should_BeIdempotent()
    {
        var purchaseRepository = new FakeIapPurchaseRepository();
        var notificationRepository = new FakePaymentNotificationRepository();
        var verifier = new FakeGoogleRealTimeDeveloperNotificationVerifier();
        verifier.Result = PaymentNotificationVerificationResult.Verified(
            PaymentPlatform.Google,
            PaymentEnvironment.Sandbox,
            notificationId: "google-message-1",
            notificationType: "ONE_TIME_PRODUCT_CANCELED",
            payloadJson: "{\"notificationType\":\"ONE_TIME_PRODUCT_CANCELED\"}",
            storeTransactionId: null,
            purchaseToken: "missing-token",
            PaymentNotificationPurchaseStatus.Revoked,
            reason: "Cancelled by Google Play.");
        var handler = new ReceiveGooglePaymentNotificationCommandHandler(
            verifier,
            new ReceivePaymentNotificationProcessor(
                notificationRepository,
                purchaseRepository,
                new FixedClock(new DateTime(2026, 5, 28, 12, 2, 0, DateTimeKind.Utc))));

        var first = await handler.Handle(
            new ReceiveGooglePaymentNotificationCommand(
                "google-message-1",
                "{\"notificationType\":\"ONE_TIME_PRODUCT_CANCELED\"}",
                authorizationToken: "Bearer test"),
            CancellationToken.None);
        var replay = await handler.Handle(
            new ReceiveGooglePaymentNotificationCommand(
                "google-message-1",
                "{\"notificationType\":\"ONE_TIME_PRODUCT_CANCELED\"}",
                authorizationToken: "Bearer test"),
            CancellationToken.None);

        first.IsReplay.Should().BeFalse();
        first.Status.Should().Be(PaymentNotificationStatus.Failed.ToString());
        replay.IsReplay.Should().BeTrue();
        replay.NotificationId.Should().Be(first.NotificationId);
        notificationRepository.Notifications.Should().ContainSingle();
    }

    private static VerifyIapPurchaseCommandHandler CreateHandler(
        FakePaymentProductRepository productRepository,
        FakeIapPurchaseRepository purchaseRepository,
        FakeAppleIapVerifier appleVerifier,
        FakeDiamondGrant diamondGrant,
        FakeGooglePlayIapVerifier? googleVerifier = null,
        FakeGooglePlayPurchaseProcessor? googleProcessor = null) =>
        new(
            productRepository,
            purchaseRepository,
            appleVerifier,
            googleVerifier ?? new FakeGooglePlayIapVerifier(),
            googleProcessor ?? new FakeGooglePlayPurchaseProcessor(),
            diamondGrant,
            new FixedClock(new DateTime(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc)));

    private sealed class FakePaymentProductRepository : IPaymentProductRepository
    {
        private readonly List<PaymentProduct> _products = [];

        public void Add(PaymentProduct product) => _products.Add(product);

        public Task<PaymentProduct?> GetByIdAsync(
            PaymentProductId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_products.SingleOrDefault(x => x.Id == id));

        public Task<PaymentProduct?> GetByStoreProductIdAsync(
            StoreProductId storeProductId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_products.SingleOrDefault(x => x.StoreProductId.Value == storeProductId.Value));

        public Task AddAsync(PaymentProduct product, CancellationToken cancellationToken = default)
        {
            _products.Add(product);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeIapPurchaseRepository : IIapPurchaseRepository
    {
        private readonly List<IapPurchase> _purchases = [];

        public IReadOnlyList<IapPurchase> Purchases => _purchases;

        public Task<IapPurchase?> GetByIdAsync(
            IapPurchaseId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_purchases.SingleOrDefault(x => x.Id == id));

        public Task<IapPurchase?> GetByStoreTransactionIdAsync(
            PaymentPlatform platform,
            StoreTransactionId storeTransactionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_purchases.SingleOrDefault(
                x => x.Platform == platform && x.StoreTransactionId?.Value == storeTransactionId.Value));

        public Task<IapPurchase?> GetByPurchaseTokenAsync(
            PaymentPlatform platform,
            PurchaseToken purchaseToken,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_purchases.SingleOrDefault(
                x => x.Platform == platform && x.PurchaseToken?.Value == purchaseToken.Value));

        public Task<IapPurchase?> GetByPlayerAndClientRequestIdAsync(
            Guid playerId,
            string clientRequestId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_purchases.SingleOrDefault(
                x => x.PlayerId == playerId && x.ClientRequestId == clientRequestId));

        public Task AddAsync(IapPurchase purchase, CancellationToken cancellationToken = default)
        {
            _purchases.Add(purchase);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePaymentNotificationRepository : IPaymentNotificationRepository
    {
        private readonly List<PaymentNotification> _notifications = [];

        public IReadOnlyList<PaymentNotification> Notifications => _notifications;

        public Task<PaymentNotification?> GetByPlatformAndNotificationIdAsync(
            PaymentPlatform platform,
            string notificationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_notifications.SingleOrDefault(
                x => x.Platform == platform && x.NotificationId == notificationId));

        public Task AddAsync(PaymentNotification notification, CancellationToken cancellationToken = default)
        {
            _notifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAppleServerNotificationVerifier : IAppleServerNotificationVerifier
    {
        public PaymentNotificationVerificationResult Result { get; set; } = null!;

        public Task<PaymentNotificationVerificationResult> VerifyAsync(
            AppleServerNotificationVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result);
    }

    private sealed class FakeGoogleRealTimeDeveloperNotificationVerifier
        : IGoogleRealTimeDeveloperNotificationVerifier
    {
        public PaymentNotificationVerificationResult Result { get; set; } = null!;

        public Task<PaymentNotificationVerificationResult> VerifyAsync(
            GoogleRealTimeDeveloperNotificationVerificationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result);
    }

    private sealed class FakeDiamondGrant : IDiamondGrant
    {
        public bool ShouldFail { get; init; }
        public List<(Guid PlayerId, int Amount)> Grants { get; } = [];

        public Task GrantAsync(Guid playerId, int amount, CancellationToken cancellationToken = default)
        {
            Grants.Add((playerId, amount));

            if (ShouldFail)
            {
                throw new InvalidOperationException("Diamond module unavailable.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }
}
