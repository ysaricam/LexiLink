using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Administration.Application.AdminActions.GetAdminActions;
using LexiLink.Modules.Administration.Application.Contracts;

namespace LexiLink.API.Modules.Admin;

public static class AdminAuditEndpoints
{
    public static void MapAdminAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/audit")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "/",
            async (
                IAdministrationModule administration,
                Guid? adminUserId,
                string? targetType,
                string? targetId,
                int? offset,
                int? limit,
                CancellationToken ct) =>
            {
                var actions = await administration.ExecuteQueryAsync(
                    new GetAdminActionsQuery(
                        adminUserId,
                        targetType,
                        targetId,
                        offset ?? 0,
                        limit ?? GetAdminActionsQuery.DefaultLimit),
                    ct);

                return Results.Ok(actions);
            })
            .Produces<IReadOnlyList<AdminActionDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }
}
