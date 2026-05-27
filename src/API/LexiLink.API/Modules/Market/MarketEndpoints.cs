using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Market.Application.Catalog;
using LexiLink.Modules.Market.Application.Catalog.GetMarketItem;
using LexiLink.Modules.Market.Application.Catalog.GetVisibleMarketCategories;
using LexiLink.Modules.Market.Application.Contracts;
using LexiLink.Modules.Market.Application.Orders;
using LexiLink.Modules.Market.Application.Orders.GetMyMarketOrders;
using LexiLink.Modules.Market.Application.Purchases.BuyShopItem;

namespace LexiLink.API.Modules.Market;

public static class MarketEndpoints
{
    public static IEndpointRouteBuilder MapMarketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/market")
            .WithTags("Market")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/items/{id:guid}/buy", async (
            Guid id,
            BuyShopItemRequest body,
            IExecutionContextAccessor executionContextAccessor,
            IMarketModule marketModule,
            CancellationToken cancellationToken) =>
        {
            var result = await marketModule.ExecuteCommandAsync(
                new BuyShopItemCommand(
                    executionContextAccessor.UserId,
                    id,
                    body.IdempotencyKey),
                cancellationToken);

            return result.IsReplay
                ? Results.Ok(result)
                : Results.Created($"/market/orders/{result.PurchaseOrderId}", result);
        });

        group.MapGet("/categories", async (
            IExecutionContextAccessor executionContextAccessor,
            IMarketModule marketModule,
            CancellationToken cancellationToken) =>
        {
            var categories = await marketModule.ExecuteQueryAsync(
                new GetVisibleMarketCategoriesQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(categories);
        })
        .Produces<IReadOnlyList<MarketCategoryDto>>();

        group.MapGet("/items/{id:guid}", async (
            Guid id,
            IExecutionContextAccessor executionContextAccessor,
            IMarketModule marketModule,
            CancellationToken cancellationToken) =>
        {
            var item = await marketModule.ExecuteQueryAsync(
                new GetMarketItemQuery(executionContextAccessor.UserId, id),
                cancellationToken);

            return Results.Ok(item);
        })
        .Produces<MarketItemDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/orders/me", async (
            int? limit,
            int? offset,
            IExecutionContextAccessor executionContextAccessor,
            IMarketModule marketModule,
            CancellationToken cancellationToken) =>
        {
            var orders = await marketModule.ExecuteQueryAsync(
                new GetMyMarketOrdersQuery(
                    executionContextAccessor.UserId,
                    limit ?? 50,
                    offset ?? 0),
                cancellationToken);

            return Results.Ok(orders);
        })
        .Produces<IReadOnlyList<MarketOrderDto>>();

        return app;
    }

    public sealed record BuyShopItemRequest(string IdempotencyKey);
}
