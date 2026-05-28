using LexiLink.Modules.Payments.Application.Configuration.Verification;
using LexiLink.Modules.Payments.Domain;
using LexiLink.Modules.Payments.Tests.Fakes;

namespace LexiLink.Modules.Payments.Tests;

[TestFixture]
public class PaymentVerificationContractTests
{
    [Test]
    public async Task FakeAppleVerifier_Should_ReturnConfiguredVerifiedTransaction()
    {
        var verifier = new FakeAppleIapVerifier();
        verifier.AddVerified(
            transactionId: "apple-transaction-1",
            storeProductId: "diamond_100",
            accountToken: Guid.NewGuid().ToString());

        var result = await verifier.VerifyAsync(new AppleIapVerificationRequest(
            Guid.NewGuid(),
            "diamond_100",
            "apple-transaction-1",
            SignedTransactionJws: "signed-jws",
            AppAccountToken: null));

        result.IsVerified.Should().BeTrue();
        result.Platform.Should().Be(PaymentPlatform.Apple);
        result.StoreTransactionId.Should().Be("apple-transaction-1");
        result.PostProcessingAction.Should().Be(StorePurchasePostProcessingAction.AppleClientFinishTransaction);
    }

    [Test]
    public async Task FakeGoogleVerifier_Should_ReturnConfiguredPendingFailure()
    {
        var verifier = new FakeGooglePlayIapVerifier();
        verifier.AddFailure(
            purchaseToken: "google-token-1",
            storeProductId: "diamond_100",
            StorePurchaseState.Pending,
            "Purchase is pending.");

        var result = await verifier.VerifyAsync(new GooglePlayIapVerificationRequest(
            Guid.NewGuid(),
            "diamond_100",
            "google-token-1",
            ObfuscatedAccountId: null,
            ObfuscatedProfileId: null));

        result.IsVerified.Should().BeFalse();
        result.Platform.Should().Be(PaymentPlatform.Google);
        result.PurchaseToken.Should().Be("google-token-1");
        result.PurchaseState.Should().Be(StorePurchaseState.Pending);
    }

    [Test]
    public async Task FakeGoogleProcessor_Should_BeIdempotent()
    {
        var processor = new FakeGooglePlayPurchaseProcessor();

        var first = await processor.ConsumeAsync("diamond_100", "google-token-1");
        var replay = await processor.ConsumeAsync("diamond_100", "google-token-1");

        first.Succeeded.Should().BeTrue();
        first.IsReplay.Should().BeFalse();
        replay.Succeeded.Should().BeTrue();
        replay.IsReplay.Should().BeTrue();
        processor.ConsumedTokens.Should().ContainSingle().Which.Should().Be("google-token-1");
    }
}
