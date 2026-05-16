using LexiLink.API.Configuration.Authentication;
using LexiLink.Common.Application;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Application.PlayerQuests.ClaimQuest;
using LexiLink.Modules.Quests.Application.PlayerQuests.GetActiveQuests;

namespace LexiLink.API.Modules.Quests;

public static class QuestEndpoints
{
    public static IEndpointRouteBuilder MapQuestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/quests")
            .WithTags("Quests")
            .RequireAuthorization(AuthConstants.AuthenticatedPlayerPolicy)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/me", async (
            IExecutionContextAccessor executionContextAccessor,
            IQuestsModule questsModule,
            CancellationToken cancellationToken) =>
        {
            var quests = await questsModule.ExecuteQueryAsync(
                new GetActiveQuestsQuery(executionContextAccessor.UserId),
                cancellationToken);

            return Results.Ok(quests);
        });

        group.MapPost("/{id:guid}/claim", async (
            Guid id,
            IExecutionContextAccessor executionContextAccessor,
            IQuestsModule questsModule,
            CancellationToken cancellationToken) =>
        {
            await questsModule.ExecuteCommandAsync(
                new ClaimQuestCommand(id, executionContextAccessor.UserId),
                cancellationToken);

            return Results.NoContent();
        });

        return app;
    }
}
