using LexiLink.Common.Domain;
using LexiLink.Modules.Payments.Domain.Events;
using LexiLink.Modules.Payments.Domain.Rules;

namespace LexiLink.Modules.Payments.Domain;

public class IapPurchase : Entity, IAggregateRoot
{
    private const int FailureReasonMaxLength = 1000;
    private const int OrderIdMaxLength = 256;
    private const int ClientRequestIdMaxLength = 128;

    public IapPurchaseId Id { get; private set; }
    private Guid _playerId;
    private PaymentPlatform _platform;
    private PaymentEnvironment _environment;
    private StoreProductId _storeProductId = null!;
    private StoreTransactionId? _storeTransactionId;
    private PurchaseToken? _purchaseToken;
    private string? _orderId;
    private string? _clientRequestId;
    private int _diamondAmount;
    private IapPurchaseStatus _status;
    private IapPurchasePostProcessingAction _postProcessingAction;
    private IapPurchasePostProcessingStatus _postProcessingStatus;
    private DateTime _receivedAt;
    private DateTime? _verifiedAt;
    private DateTime? _grantedAt;
    private string? _failureReason;
    private DateTime? _postProcessedAt;
    private string? _postProcessingFailureReason;

    public Guid PlayerId => _playerId;
    public PaymentPlatform Platform => _platform;
    public PaymentEnvironment Environment => _environment;
    public StoreProductId StoreProductId => _storeProductId;
    public StoreTransactionId? StoreTransactionId => _storeTransactionId;
    public PurchaseToken? PurchaseToken => _purchaseToken;
    public string? OrderId => _orderId;
    public string? ClientRequestId => _clientRequestId;
    public int DiamondAmount => _diamondAmount;
    public IapPurchaseStatus Status => _status;
    public IapPurchasePostProcessingAction PostProcessingAction => _postProcessingAction;
    public IapPurchasePostProcessingStatus PostProcessingStatus => _postProcessingStatus;
    public DateTime ReceivedAt => _receivedAt;
    public DateTime? VerifiedAt => _verifiedAt;
    public DateTime? GrantedAt => _grantedAt;
    public string? FailureReason => _failureReason;
    public DateTime? PostProcessedAt => _postProcessedAt;
    public string? PostProcessingFailureReason => _postProcessingFailureReason;

    private IapPurchase()
    {
        Id = null!;
    }

    private IapPurchase(
        IapPurchaseId id,
        Guid playerId,
        PaymentPlatform platform,
        PaymentEnvironment environment,
        StoreProductId storeProductId,
        StoreTransactionId? storeTransactionId,
        PurchaseToken? purchaseToken,
        string? orderId,
        string? clientRequestId,
        int diamondAmount,
        IapPurchasePostProcessingAction postProcessingAction,
        DateTime receivedAt)
    {
        Id = id;
        _playerId = playerId;
        _platform = platform;
        _environment = environment;
        _storeProductId = storeProductId;
        _storeTransactionId = storeTransactionId;
        _purchaseToken = purchaseToken;
        _orderId = NormalizeOptional(orderId);
        _clientRequestId = NormalizeOptional(clientRequestId);
        _diamondAmount = diamondAmount;
        _postProcessingAction = postProcessingAction;
        _postProcessingStatus = postProcessingAction == IapPurchasePostProcessingAction.None
            ? IapPurchasePostProcessingStatus.None
            : IapPurchasePostProcessingStatus.Pending;
        _status = IapPurchaseStatus.Received;
        _receivedAt = receivedAt;

        AddDomainEvent(new IapPurchaseReceivedDomainEvent(
            Id.Value,
            _playerId,
            _platform,
            _storeProductId.Value));
    }

    internal static IapPurchase CreateReceived(
        Guid playerId,
        PaymentPlatform platform,
        PaymentEnvironment environment,
        StoreProductId storeProductId,
        StoreTransactionId? storeTransactionId,
        PurchaseToken? purchaseToken,
        string? orderId,
        string? clientRequestId,
        int diamondAmount,
        IapPurchasePostProcessingAction postProcessingAction,
        DateTime receivedAt)
    {
        CheckRule(new StoreProofMustMatchPlatformRule(platform, storeTransactionId, purchaseToken));
        CheckRule(new AmountMustBePositiveRule(diamondAmount, nameof(diamondAmount)));
        CheckRule(new TextMustNotExceedMaxLengthRule(NormalizeOptional(orderId), OrderIdMaxLength, nameof(orderId)));
        CheckRule(new TextMustNotExceedMaxLengthRule(NormalizeOptional(clientRequestId), ClientRequestIdMaxLength, nameof(clientRequestId)));

        return new IapPurchase(
            new IapPurchaseId(Guid.NewGuid()),
            playerId,
            platform,
            environment,
            storeProductId,
            storeTransactionId,
            purchaseToken,
            orderId,
            clientRequestId,
            diamondAmount,
            postProcessingAction,
            receivedAt);
    }

    internal void MarkVerified(DateTime verifiedAt)
    {
        _status = IapPurchaseStatus.Verified;
        _verifiedAt = verifiedAt;
        _failureReason = null;

        AddDomainEvent(new IapPurchaseStatusChangedDomainEvent(Id.Value, _status));
    }

    internal void MarkGranted(DateTime grantedAt)
    {
        _status = IapPurchaseStatus.Granted;
        _grantedAt = grantedAt;
        _failureReason = null;

        AddDomainEvent(new IapPurchaseStatusChangedDomainEvent(Id.Value, _status));
        AddDomainEvent(new IapPurchaseGrantedDomainEvent(
            Id.Value,
            _playerId,
            _platform,
            _storeProductId.Value,
            _diamondAmount,
            grantedAt));
    }

    internal void MarkPostProcessingSucceeded(DateTime processedAt)
    {
        _postProcessingStatus = _postProcessingAction == IapPurchasePostProcessingAction.None
            ? IapPurchasePostProcessingStatus.None
            : IapPurchasePostProcessingStatus.Succeeded;
        _postProcessedAt = processedAt;
        _postProcessingFailureReason = null;
    }

    internal void MarkPostProcessingFailed(string failureReason)
    {
        var normalized = NormalizeOptional(failureReason);
        CheckRule(new TextMustNotBeEmptyRule(normalized, nameof(failureReason)));
        CheckRule(new TextMustNotExceedMaxLengthRule(normalized, FailureReasonMaxLength, nameof(failureReason)));

        _postProcessingStatus = IapPurchasePostProcessingStatus.Failed;
        _postProcessingFailureReason = normalized;
    }

    internal void MarkVerifiedButGrantFailed(string failureReason)
    {
        SetFailure(IapPurchaseStatus.VerifiedButGrantFailed, failureReason);
    }

    internal void MarkFailed(string failureReason)
    {
        SetFailure(IapPurchaseStatus.Failed, failureReason);
    }

    internal void MarkRefunded(string reason)
    {
        SetFailure(IapPurchaseStatus.Refunded, reason);
    }

    internal void MarkRevoked(string reason)
    {
        SetFailure(IapPurchaseStatus.Revoked, reason);
    }

    private void SetFailure(IapPurchaseStatus status, string reason)
    {
        var normalized = NormalizeOptional(reason);
        CheckRule(new TextMustNotBeEmptyRule(normalized, nameof(reason)));
        CheckRule(new TextMustNotExceedMaxLengthRule(normalized, FailureReasonMaxLength, nameof(reason)));

        _status = status;
        _failureReason = normalized;

        AddDomainEvent(new IapPurchaseStatusChangedDomainEvent(Id.Value, _status));
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
