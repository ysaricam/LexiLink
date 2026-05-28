using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Common.Domain;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Payments.Application.Configuration.Commands;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Application.IapPurchases;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;

internal sealed class VerifyIapPurchaseCommandHandler
    : ICommandHandler<VerifyIapPurchaseCommand, VerifyIapPurchaseResultDto>
{
    private readonly IPaymentProductRepository _productRepository;
    private readonly IIapPurchaseRepository _purchaseRepository;
    private readonly IAppleIapVerifier _appleVerifier;
    private readonly IGooglePlayIapVerifier _googleVerifier;
    private readonly IGooglePlayPurchaseProcessor _googleProcessor;
    private readonly IDiamondGrant _diamondGrant;
    private readonly IClock _clock;

    internal VerifyIapPurchaseCommandHandler(
        IPaymentProductRepository productRepository,
        IIapPurchaseRepository purchaseRepository,
        IAppleIapVerifier appleVerifier,
        IGooglePlayIapVerifier googleVerifier,
        IGooglePlayPurchaseProcessor googleProcessor,
        IDiamondGrant diamondGrant,
        IClock clock)
    {
        _productRepository = productRepository;
        _purchaseRepository = purchaseRepository;
        _appleVerifier = appleVerifier;
        _googleVerifier = googleVerifier;
        _googleProcessor = googleProcessor;
        _diamondGrant = diamondGrant;
        _clock = clock;
    }

    public async Task<VerifyIapPurchaseResultDto> Handle(
        VerifyIapPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await FindExistingAsync(request, cancellationToken);
        if (existing is not null)
        {
            CheckRule(new ExistingPurchaseMustBelongToPlayerRule(existing.PlayerId, request.PlayerId));
            return IapPurchaseResultFactory.ToVerifyResult(existing, isReplay: true);
        }

        var storeProductId = StoreProductId.Of(request.StoreProductId);
        var product = await _productRepository.GetByStoreProductIdAsync(storeProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(PaymentProduct), request.StoreProductId);

        CheckRule(new PaymentProductMustBeActiveRule(product));
        CheckRule(new PaymentProductMustSupportPlatformRule(product, request.Platform));

        var verificationResult = await VerifyWithStoreAsync(request, cancellationToken);
        CheckRule(new VerificationResultMustMatchPlatformRule(verificationResult.Platform, request.Platform));
        CheckRule(new VerificationResultMustMatchProductRule(
            verificationResult.StoreProductId,
            product.StoreProductId.Value));

        var purchase = IapPurchase.CreateReceived(
            request.PlayerId,
            request.Platform,
            verificationResult.Environment,
            product.StoreProductId,
            ResolveStoreTransactionId(request, verificationResult),
            ResolvePurchaseToken(request, verificationResult),
            verificationResult.OrderId,
            request.ClientRequestId,
            product.DiamondAmount,
            IapPurchasePostProcessing.MapAction(verificationResult.PostProcessingAction),
            _clock.UtcNow);

        await _purchaseRepository.AddAsync(purchase, cancellationToken);

        if (!verificationResult.IsVerified ||
            verificationResult.PurchaseState != StorePurchaseState.Purchased)
        {
            purchase.MarkFailed(verificationResult.FailureReason ?? $"Store purchase state is {verificationResult.PurchaseState}.");
            return IapPurchaseResultFactory.ToVerifyResult(purchase, isReplay: false);
        }

        CheckRule(new StoreAccountTokenMustMatchPlayerRule(verificationResult.AccountToken, request.PlayerId));

        purchase.MarkVerified(_clock.UtcNow);

        try
        {
            await _diamondGrant.GrantAsync(
                request.PlayerId,
                product.DiamondAmount,
                cancellationToken);
        }
        catch (Exception ex)
        {
            purchase.MarkVerifiedButGrantFailed(ex.Message);
            return IapPurchaseResultFactory.ToVerifyResult(purchase, isReplay: false);
        }

        purchase.MarkGranted(_clock.UtcNow);
        await IapPurchasePostProcessing.ProcessAsync(
            purchase,
            _googleProcessor,
            _clock.UtcNow,
            cancellationToken);

        return IapPurchaseResultFactory.ToVerifyResult(purchase, isReplay: false);
    }

    private async Task<IapPurchase?> FindExistingAsync(
        VerifyIapPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.ClientRequestId))
        {
            var existingByClientRequest = await _purchaseRepository.GetByPlayerAndClientRequestIdAsync(
                request.PlayerId,
                request.ClientRequestId.Trim(),
                cancellationToken);
            if (existingByClientRequest is not null)
            {
                return existingByClientRequest;
            }
        }

        return request.Platform switch
        {
            PaymentPlatform.Apple when !string.IsNullOrWhiteSpace(request.StoreTransactionId) =>
                await _purchaseRepository.GetByStoreTransactionIdAsync(
                    request.Platform,
                    StoreTransactionId.Of(request.StoreTransactionId),
                    cancellationToken),
            PaymentPlatform.Google when !string.IsNullOrWhiteSpace(request.PurchaseToken) =>
                await _purchaseRepository.GetByPurchaseTokenAsync(
                    request.Platform,
                    PurchaseToken.Of(request.PurchaseToken),
                    cancellationToken),
            _ => null
        };
    }

    private Task<StorePurchaseVerificationResult> VerifyWithStoreAsync(
        VerifyIapPurchaseCommand request,
        CancellationToken cancellationToken) =>
        request.Platform switch
        {
            PaymentPlatform.Apple => _appleVerifier.VerifyAsync(
                new AppleIapVerificationRequest(
                    request.PlayerId,
                    request.StoreProductId,
                    request.StoreTransactionId!,
                    request.SignedTransactionJws,
                    request.AccountToken),
                cancellationToken),
            PaymentPlatform.Google => _googleVerifier.VerifyAsync(
                new GooglePlayIapVerificationRequest(
                    request.PlayerId,
                    request.StoreProductId,
                    request.PurchaseToken!,
                    request.AccountToken,
                    ObfuscatedProfileId: null),
                cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported payment platform '{request.Platform}'.")
        };

    private static StoreTransactionId? ResolveStoreTransactionId(
        VerifyIapPurchaseCommand request,
        StorePurchaseVerificationResult result)
    {
        var value = result.StoreTransactionId ?? request.StoreTransactionId;
        return string.IsNullOrWhiteSpace(value) ? null : StoreTransactionId.Of(value);
    }

    private static PurchaseToken? ResolvePurchaseToken(
        VerifyIapPurchaseCommand request,
        StorePurchaseVerificationResult result)
    {
        var value = result.PurchaseToken ?? request.PurchaseToken;
        return string.IsNullOrWhiteSpace(value) ? null : PurchaseToken.Of(value);
    }

    private static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }

    private sealed class ExistingPurchaseMustBelongToPlayerRule : IBusinessRule
    {
        private readonly Guid _existingPlayerId;
        private readonly Guid _requestPlayerId;

        public ExistingPurchaseMustBelongToPlayerRule(Guid existingPlayerId, Guid requestPlayerId)
        {
            _existingPlayerId = existingPlayerId;
            _requestPlayerId = requestPlayerId;
        }

        public bool IsBroken() => _existingPlayerId != _requestPlayerId;

        public string Message => "IAP purchase proof is already bound to another player.";
    }

    private sealed class PaymentProductMustBeActiveRule : IBusinessRule
    {
        private readonly PaymentProduct _product;

        public PaymentProductMustBeActiveRule(PaymentProduct product)
        {
            _product = product;
        }

        public bool IsBroken() => !_product.IsActive;

        public string Message => "Payment product must be active.";
    }

    private sealed class PaymentProductMustSupportPlatformRule : IBusinessRule
    {
        private readonly PaymentProduct _product;
        private readonly PaymentPlatform _platform;

        public PaymentProductMustSupportPlatformRule(PaymentProduct product, PaymentPlatform platform)
        {
            _product = product;
            _platform = platform;
        }

        public bool IsBroken() => !_product.Supports(_platform);

        public string Message => "Payment product is not available on the requested platform.";
    }

    private sealed class VerificationResultMustMatchPlatformRule : IBusinessRule
    {
        private readonly PaymentPlatform _actual;
        private readonly PaymentPlatform _expected;

        public VerificationResultMustMatchPlatformRule(PaymentPlatform actual, PaymentPlatform expected)
        {
            _actual = actual;
            _expected = expected;
        }

        public bool IsBroken() => _actual != _expected;

        public string Message => "Store verification result platform does not match the requested platform.";
    }

    private sealed class VerificationResultMustMatchProductRule : IBusinessRule
    {
        private readonly string _actual;
        private readonly string _expected;

        public VerificationResultMustMatchProductRule(string actual, string expected)
        {
            _actual = actual;
            _expected = expected;
        }

        public bool IsBroken() => !string.Equals(_actual, _expected, StringComparison.Ordinal);

        public string Message => "Store verification result product does not match the requested product.";
    }

    private sealed class StoreAccountTokenMustMatchPlayerRule : IBusinessRule
    {
        private readonly string? _accountToken;
        private readonly Guid _playerId;

        public StoreAccountTokenMustMatchPlayerRule(string? accountToken, Guid playerId)
        {
            _accountToken = accountToken;
            _playerId = playerId;
        }

        public bool IsBroken() =>
            !string.IsNullOrWhiteSpace(_accountToken) &&
            !string.Equals(_accountToken.Trim(), _playerId.ToString(), StringComparison.OrdinalIgnoreCase);

        public string Message => "Store account token does not match the authenticated player.";
    }
}
