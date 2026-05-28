using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Payments.Application.Admin.IapPurchases;
using LexiLink.Modules.Payments.Application.Admin.IapPurchases.GetAdminIapPurchase;
using LexiLink.Modules.Payments.Application.Admin.IapPurchases.GetAdminIapPurchases;
using LexiLink.Modules.Payments.Application.Admin.PaymentProducts.CreatePaymentProduct;
using LexiLink.Modules.Payments.Application.Admin.PaymentProducts.DeactivatePaymentProduct;
using LexiLink.Modules.Payments.Application.Admin.PaymentProducts.GetAdminPaymentProduct;
using LexiLink.Modules.Payments.Application.Admin.PaymentProducts.GetAdminPaymentProducts;
using LexiLink.Modules.Payments.Application.Admin.PaymentProducts.UpdatePaymentProduct;
using LexiLink.Modules.Payments.Application.Contracts;
using LexiLink.Modules.Payments.Application.IapPurchases.RetryIapPurchaseDelivery;
using LexiLink.Modules.Payments.Application.IapPurchases.VerifyIapPurchase;
using LexiLink.Modules.Payments.Application.PaymentProducts;
using LexiLink.Modules.Payments.Domain;

namespace LexiLink.API.Modules.Admin;

public static class AdminPaymentsEndpoints
{
    public static void MapAdminPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/payments")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "/products",
            async (
                PaymentPlatform? platform,
                bool? isActive,
                IPaymentsModule payments,
                CancellationToken ct) =>
            {
                var products = await payments.ExecuteQueryAsync(
                    new GetAdminPaymentProductsQuery(platform, isActive),
                    ct);
                return Results.Ok(products);
            })
            .Produces<IReadOnlyList<PaymentProductDto>>();

        group.MapGet(
            "/products/{id:guid}",
            async (IPaymentsModule payments, Guid id, CancellationToken ct) =>
            {
                var product = await payments.ExecuteQueryAsync(new GetAdminPaymentProductQuery(id), ct);
                return Results.Ok(product);
            })
            .Produces<PaymentProductDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/products",
            async (IPaymentsModule payments, CreatePaymentProductRequest body, CancellationToken ct) =>
            {
                var id = await payments.ExecuteCommandAsync(
                    new CreatePaymentProductCommand(
                        body.StoreProductId,
                        body.DiamondAmount,
                        body.IsAppleAvailable,
                        body.IsGoogleAvailable,
                        body.SortOrder),
                    ct);

                return Results.Created($"/admin/payments/products/{id}", new CreatePaymentResourceResponse(id));
            })
            .Produces<CreatePaymentResourceResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut(
            "/products/{id:guid}",
            async (IPaymentsModule payments, Guid id, UpdatePaymentProductRequest body, CancellationToken ct) =>
            {
                await payments.ExecuteCommandAsync(
                    new UpdatePaymentProductCommand(
                        id,
                        body.DiamondAmount,
                        body.IsAppleAvailable,
                        body.IsGoogleAvailable,
                        body.SortOrder),
                    ct);

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/products/{id:guid}/deactivate",
            async (IPaymentsModule payments, Guid id, CancellationToken ct) =>
            {
                await payments.ExecuteCommandAsync(new DeactivatePaymentProductCommand(id), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet(
            "/purchases",
            async (
                Guid? playerId,
                PaymentPlatform? platform,
                IapPurchaseStatus? status,
                string? storeProductId,
                int? limit,
                int? offset,
                IPaymentsModule payments,
                CancellationToken ct) =>
            {
                var purchases = await payments.ExecuteQueryAsync(
                    new GetAdminIapPurchasesQuery(
                        playerId,
                        platform,
                        status,
                        storeProductId,
                        limit ?? 50,
                        offset ?? 0),
                    ct);

                return Results.Ok(purchases);
            })
            .Produces<IReadOnlyList<AdminIapPurchaseDto>>();

        group.MapGet(
            "/purchases/{id:guid}",
            async (IPaymentsModule payments, Guid id, CancellationToken ct) =>
            {
                var purchase = await payments.ExecuteQueryAsync(new GetAdminIapPurchaseQuery(id), ct);
                return Results.Ok(purchase);
            })
            .Produces<AdminIapPurchaseDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/purchases/{id:guid}/retry-delivery",
            async (IPaymentsModule payments, Guid id, CancellationToken ct) =>
            {
                var result = await payments.ExecuteCommandAsync(
                    new RetryIapPurchaseDeliveryCommand(id),
                    ct);
                return Results.Ok(result);
            })
            .Produces<VerifyIapPurchaseResultDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record CreatePaymentResourceResponse(Guid Id);

public sealed record CreatePaymentProductRequest(
    string StoreProductId,
    int DiamondAmount,
    bool IsAppleAvailable,
    bool IsGoogleAvailable,
    int SortOrder);

public sealed record UpdatePaymentProductRequest(
    int DiamondAmount,
    bool IsAppleAvailable,
    bool IsGoogleAvailable,
    int SortOrder);
