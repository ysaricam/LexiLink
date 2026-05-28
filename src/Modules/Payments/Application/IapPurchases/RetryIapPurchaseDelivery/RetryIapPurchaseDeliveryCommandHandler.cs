using LexiLink.Common.Application.Exceptions;
using LexiLink.Common.Application.Time;
using LexiLink.Modules.Diamond.Application.Configuration.CrossModule;
using LexiLink.Modules.Payments.Application.Configuration.Commands;
using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.Modules.Payments.Application.IapPurchases.RetryIapPurchaseDelivery;

internal sealed class RetryIapPurchaseDeliveryCommandHandler
    : ICommandHandler<RetryIapPurchaseDeliveryCommand, VerifyIapPurchaseResultDto>
{
    private readonly IIapPurchaseRepository _purchaseRepository;
    private readonly IDiamondGrant _diamondGrant;
    private readonly IGooglePlayPurchaseProcessor _googleProcessor;
    private readonly IClock _clock;

    internal RetryIapPurchaseDeliveryCommandHandler(
        IIapPurchaseRepository purchaseRepository,
        IDiamondGrant diamondGrant,
        IGooglePlayPurchaseProcessor googleProcessor,
        IClock clock)
    {
        _purchaseRepository = purchaseRepository;
        _diamondGrant = diamondGrant;
        _googleProcessor = googleProcessor;
        _clock = clock;
    }

    public async Task<VerifyIapPurchaseResultDto> Handle(
        RetryIapPurchaseDeliveryCommand request,
        CancellationToken cancellationToken)
    {
        var purchase = await _purchaseRepository.GetByIdAsync(
            new IapPurchaseId(request.IapPurchaseId),
            cancellationToken)
            ?? throw new NotFoundException(nameof(IapPurchase), request.IapPurchaseId);

        if (purchase.Status == IapPurchaseStatus.VerifiedButGrantFailed)
        {
            try
            {
                await _diamondGrant.GrantAsync(
                    purchase.PlayerId,
                    purchase.DiamondAmount,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                purchase.MarkVerifiedButGrantFailed(ex.Message);
                return IapPurchaseResultFactory.ToVerifyResult(purchase, isReplay: false);
            }

            purchase.MarkGranted(_clock.UtcNow);
        }

        if (purchase.Status == IapPurchaseStatus.Granted &&
            purchase.PostProcessingAction != IapPurchasePostProcessingAction.None &&
            purchase.PostProcessingStatus != IapPurchasePostProcessingStatus.Succeeded)
        {
            await IapPurchasePostProcessing.ProcessAsync(
                purchase,
                _googleProcessor,
                _clock.UtcNow,
                cancellationToken);
        }

        return IapPurchaseResultFactory.ToVerifyResult(purchase, isReplay: false);
    }
}
