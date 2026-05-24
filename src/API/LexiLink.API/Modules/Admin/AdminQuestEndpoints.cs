using LexiLink.API.Configuration.Authentication;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.DeactivateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.GetQuestDefinitions;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.ReactivateQuestDefinition;
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
                        body.Name,
                        body.Description,
                        body.Trigger,
                        body.Threshold,
                        body.Reward,
                        body.PrerequisiteQuestDefinitionId,
                        body.ProgressBaseline),
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
                        body.Description,
                        body.Threshold,
                        body.Reward,
                        body.PrerequisiteQuestDefinitionId,
                        body.ProgressBaseline),
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
            "/definitions/{id:guid}/reactivate",
            async (IQuestsModule quests, Guid id, CancellationToken ct) =>
            {
                await quests.ExecuteCommandAsync(new ReactivateQuestDefinitionCommand(id), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record CreateQuestDefinitionRequest(
    string Name,
    string Description,
    QuestTrigger Trigger,
    int Threshold,
    int Reward,
    Guid? PrerequisiteQuestDefinitionId,
    ProgressBaseline ProgressBaseline);

public sealed record CreateQuestDefinitionResponse(Guid Id);

public sealed record UpdateQuestDefinitionRequest(
    string Description,
    int Threshold,
    int Reward,
    Guid? PrerequisiteQuestDefinitionId,
    ProgressBaseline ProgressBaseline);
