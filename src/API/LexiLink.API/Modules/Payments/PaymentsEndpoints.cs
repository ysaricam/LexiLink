using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;
using LexiLink.Modules.Payments.Application.Notifications.ReceivePaymentNotification;
using LexiLink.Modules.Payments.Application.PaymentProducts;
using LexiLink.Modules.Payments.Application.PaymentProducts.GetPaymentProducts;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.API.Modules.Payments;

public static class PaymentsEndpoints
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/payments")
            .WithTags("Payments")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/products", async (
            PaymentPlatform platform,
            IPaymentsModule paymentsModule,
            CancellationToken cancellationToken) =>
        {
            var products = await paymentsModule.ExecuteQueryAsync(
                new GetPaymentProductsQuery(platform),
                cancellationToken);

            return Results.Ok(products);
        })
        .Produces<IReadOnlyList<PaymentProductDto>>();

        group.MapPost("/iap/verify", async (
            VerifyIapPurchaseRequest body,
            IExecutionContextAccessor executionContextAccessor,
            IPaymentsModule paymentsModule,
            CancellationToken cancellationToken) =>
        {
            var result = await paymentsModule.ExecuteCommandAsync(
                new VerifyIapPurchaseCommand(
                    executionContextAccessor.UserId,
                    body.Platform,
                    body.StoreProductId,
                    body.StoreTransactionId,
                    body.PurchaseToken,
                    body.SignedTransactionJws,
                    body.AccountToken,
                    body.ClientRequestId),
                cancellationToken);

            return result is { IsReplay: false, Status: nameof(IapPurchaseStatus.Granted) }
                ? Results.Created($"/admin/payments/purchases/{result.PaymentId}", result)
                : Results.Ok(result);
        })
        .Produces<VerifyIapPurchaseResultDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/notifications/apple", async (
            ApplePaymentNotificationRequest body,
            IPaymentsModule paymentsModule,
            CancellationToken cancellationToken) =>
        {
            var result = await paymentsModule.ExecuteCommandAsync(
                new ReceiveApplePaymentNotificationCommand(body.SignedPayload),
                cancellationToken);

            return result.IsReplay ? Results.Ok(result) : Results.Accepted(value: result);
        })
        .AllowAnonymous()
        .Produces<PaymentNotificationResultDto>(StatusCodes.Status202Accepted);

        group.MapPost("/notifications/google", async (
            GooglePaymentNotificationRequest body,
            HttpContext httpContext,
            IPaymentsModule paymentsModule,
            CancellationToken cancellationToken) =>
        {
            var authorization = httpContext.Request.Headers.Authorization.ToString();
            var result = await paymentsModule.ExecuteCommandAsync(
                new ReceiveGooglePaymentNotificationCommand(
                    body.MessageId,
                    body.PayloadJson,
                    string.IsNullOrWhiteSpace(authorization) ? null : authorization),
                cancellationToken);

            return result.IsReplay ? Results.Ok(result) : Results.Accepted(value: result);
        })
        .AllowAnonymous()
        .Produces<PaymentNotificationResultDto>(StatusCodes.Status202Accepted);

        return app;
    }

    public sealed record VerifyIapPurchaseRequest(
        PaymentPlatform Platform,
        string StoreProductId,
        string? StoreTransactionId,
        string? PurchaseToken,
        string? SignedTransactionJws,
        string? AccountToken,
        string? ClientRequestId);

    public sealed record ApplePaymentNotificationRequest(string SignedPayload);

    public sealed record GooglePaymentNotificationRequest(string MessageId, string PayloadJson);
}
