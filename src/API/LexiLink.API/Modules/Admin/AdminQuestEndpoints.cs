using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Quests.Application.Admin.PlayerQuests.IssueQuestToPlayer;
using LexiLink.Modules.Quests.Application.Admin.PlayerQuests.ResetPlayerQuest;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.DeactivateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.GetQuestDefinitions;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;
using LexiLink.Modules.Quests.Application.Contracts;
using LexiLink.Modules.Quests.Domain.PlayerQuests;

namespace LexiLink.API.Modules.Admin;

public static class AdminQuestEndpoints
{
    public static void MapAdminQuestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/quests")
            .WithTags("Admin")
            .RequireAuthorization(AuthConstants.AuthenticatedAdminPolicy);

        group.MapGet(
            "/definitions",
            async (IQuestsModule quests, CancellationToken ct) =>
            {
                var defs = await quests.ExecuteQueryAsync(new GetQuestDefinitionsQuery(), ct);
                return Results.Ok(defs);
            })
            .Produces<IReadOnlyList<QuestDefinitionDto>>();

        group.MapPost(
            "/definitions",
            async (IQuestsModule quests, CreateQuestDefinitionRequest body, CancellationToken ct) =>
            {
                var id = await quests.ExecuteCommandAsync(
                    new CreateQuestDefinitionCommand(
                        body.QuestType,
                        body.Cadence,
                        body.Goal,
                        body.RewardAmount,
                        body.PrerequisiteQuestType),
                    ct);
                return Results.Created($"/admin/quests/definitions/{id}", new CreateQuestDefinitionResponse(id));
            })
            .Produces<CreateQuestDefinitionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut(
            "/definitions/{id:guid}",
            async (IQuestsModule quests, Guid id, UpdateQuestDefinitionRequest body, CancellationToken ct) =>
            {
                await quests.ExecuteCommandAsync(
                    new UpdateQuestDefinitionCommand(
                        id,
                        body.Goal,
                        body.RewardAmount,
                        body.PrerequisiteQuestType),
                    ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost(
            "/definitions/{id:guid}/deactivate",
            async (IQuestsModule quests, Guid id, CancellationToken ct) =>
            {
                await quests.ExecuteCommandAsync(new DeactivateQuestDefinitionCommand(id), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(
            "/players/{playerId:guid}/issue",
            async (IQuestsModule quests, Guid playerId, IssueQuestRequest body, CancellationToken ct) =>
            {
                await quests.ExecuteCommandAsync(
                    new IssueQuestToPlayerCommand(playerId, body.QuestType),
                    ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost(
            "/players/{playerId:guid}/{playerQuestId:guid}/reset",
            async (IQuestsModule quests, Guid playerId, Guid playerQuestId, CancellationToken ct) =>
            {
                // PlayerId is not required by the handler but is in the URL so
                // admin UIs link cleanly from the player detail page. The handler
                // looks the quest up by its own id.
                _ = playerId;
                await quests.ExecuteCommandAsync(new ResetPlayerQuestCommand(playerQuestId), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record CreateQuestDefinitionRequest(
    QuestType QuestType,
    QuestCadence Cadence,
    int Goal,
    int RewardAmount,
    QuestType? PrerequisiteQuestType);

public sealed record CreateQuestDefinitionResponse(Guid Id);

public sealed record UpdateQuestDefinitionRequest(
    int Goal,
    int RewardAmount,
    QuestType? PrerequisiteQuestType);

public sealed record IssueQuestRequest(QuestType QuestType);
