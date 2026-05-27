using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Market.Application.Admin.Catalog;
using LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketCategories;
using LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketItem;
using LexiLink.Modules.Market.Application.Admin.Catalog.GetAdminMarketItems;
using LexiLink.Modules.Market.Application.Admin.Categories.CreateCategory;
using LexiLink.Modules.Market.Application.Admin.Categories.DeactivateCategory;
using LexiLink.Modules.Market.Application.Admin.Categories.UpdateCategory;
using LexiLink.Modules.Market.Application.Admin.Orders.GetPlayerMarketOrders;
using LexiLink.Modules.Market.Application.Admin.ShopItems.CreateShopItem;
using LexiLink.Modules.Market.Application.Admin.ShopItems.DeactivateShopItem;
using LexiLink.Modules.Market.Application.Admin.ShopItems.UpdateShopItem;
using LexiLink.Modules.Market.Application.Contracts;
using LexiLink.Modules.Market.Application.Orders;
using LexiLink.Modules.Market.Domain;

namespace LexiLink.API.Modules.Admin;

public static class AdminMarketEndpoints
{
    public static void MapAdminMarketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/market")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "/categories",
            async (IMarketModule market, CancellationToken ct) =>
            {
                var categories = await market.ExecuteQueryAsync(new GetAdminMarketCategoriesQuery(), ct);
                return Results.Ok(categories);
            })
            .Produces<IReadOnlyList<AdminMarketCategoryDto>>();

        group.MapPost(
            "/categories",
            async (IMarketModule market, CreateMarketCategoryRequest body, CancellationToken ct) =>
            {
                var id = await market.ExecuteCommandAsync(
                    new CreateCategoryCommand(
                        body.Name,
                        body.SortOrder,
                        body.Icon,
                        body.VisibilityStartsAt,
                        body.VisibilityEndsAt),
                    ct);

                return Results.Created($"/admin/market/categories/{id}", new CreateMarketResourceResponse(id));
            })
            .Produces<CreateMarketResourceResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut(
            "/categories/{id:guid}",
            async (IMarketModule market, Guid id, UpdateMarketCategoryRequest body, CancellationToken ct) =>
            {
                await market.ExecuteCommandAsync(
                    new UpdateCategoryCommand(
                        id,
                        body.Name,
                        body.SortOrder,
                        body.Icon,
                        body.VisibilityStartsAt,
                        body.VisibilityEndsAt),
                    ct);

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/categories/{id:guid}/deactivate",
            async (IMarketModule market, Guid id, CancellationToken ct) =>
            {
                await market.ExecuteCommandAsync(new DeactivateCategoryCommand(id), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet(
            "/items",
            async (
                Guid? categoryId,
                ItemType? itemType,
                bool? isActive,
                IMarketModule market,
                CancellationToken ct) =>
            {
                var items = await market.ExecuteQueryAsync(
                    new GetAdminMarketItemsQuery(categoryId, itemType, isActive),
                    ct);
                return Results.Ok(items);
            })
            .Produces<IReadOnlyList<AdminMarketItemDto>>();

        group.MapGet(
            "/items/{id:guid}",
            async (IMarketModule market, Guid id, CancellationToken ct) =>
            {
                var item = await market.ExecuteQueryAsync(new GetAdminMarketItemQuery(id), ct);
                return Results.Ok(item);
            })
            .Produces<AdminMarketItemDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/items",
            async (IMarketModule market, CreateMarketShopItemRequest body, CancellationToken ct) =>
            {
                var id = await market.ExecuteCommandAsync(
                    new CreateShopItemCommand(
                        body.CategoryId,
                        body.ItemType,
                        body.Quantity,
                        body.Price,
                        body.PromoPrice,
                        body.PromotionStartsAt,
                        body.PromotionEndsAt,
                        body.MaxStock,
                        body.PerPlayerLimit,
                        body.PerPlayerLimitWindow),
                    ct);

                return Results.Created($"/admin/market/items/{id}", new CreateMarketResourceResponse(id));
            })
            .Produces<CreateMarketResourceResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPut(
            "/items/{id:guid}",
            async (IMarketModule market, Guid id, UpdateMarketShopItemRequest body, CancellationToken ct) =>
            {
                await market.ExecuteCommandAsync(
                    new UpdateShopItemCommand(
                        id,
                        body.CategoryId,
                        body.ItemType,
                        body.Quantity,
                        body.Price,
                        body.PromoPrice,
                        body.PromotionStartsAt,
                        body.PromotionEndsAt,
                        body.MaxStock,
                        body.PerPlayerLimit,
                        body.PerPlayerLimitWindow),
                    ct);

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/items/{id:guid}/deactivate",
            async (IMarketModule market, Guid id, CancellationToken ct) =>
            {
                await market.ExecuteCommandAsync(new DeactivateShopItemCommand(id), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet(
            "/orders/{playerId:guid}",
            async (
                IMarketModule market,
                Guid playerId,
                int? limit,
                int? offset,
                CancellationToken ct) =>
            {
                var orders = await market.ExecuteQueryAsync(
                    new GetPlayerMarketOrdersQuery(playerId, limit ?? 50, offset ?? 0),
                    ct);
                return Results.Ok(orders);
            })
            .Produces<IReadOnlyList<MarketOrderDto>>();
    }
}

public sealed record CreateMarketResourceResponse(Guid Id);

public sealed record CreateMarketCategoryRequest(
    string Name,
    int SortOrder,
    string? Icon,
    DateTime? VisibilityStartsAt,
    DateTime? VisibilityEndsAt);

public sealed record UpdateMarketCategoryRequest(
    string Name,
    int SortOrder,
    string? Icon,
    DateTime? VisibilityStartsAt,
    DateTime? VisibilityEndsAt);

public sealed record CreateMarketShopItemRequest(
    Guid CategoryId,
    ItemType ItemType,
    int Quantity,
    int Price,
    int? PromoPrice,
    DateTime? PromotionStartsAt,
    DateTime? PromotionEndsAt,
    int? MaxStock,
    int? PerPlayerLimit,
    PerPlayerLimitWindow PerPlayerLimitWindow);

public sealed record UpdateMarketShopItemRequest(
    Guid CategoryId,
    ItemType ItemType,
    int Quantity,
    int Price,
    int? PromoPrice,
    DateTime? PromotionStartsAt,
    DateTime? PromotionEndsAt,
    int? MaxStock,
    int? PerPlayerLimit,
    PerPlayerLimitWindow PerPlayerLimitWindow);
