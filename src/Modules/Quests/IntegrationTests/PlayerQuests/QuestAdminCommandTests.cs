using Dapper;
using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Admin.PlayerQuests.IssueQuestToPlayer;
using LexiLink.Modules.Quests.Application.Admin.PlayerQuests.ResetPlayerQuest;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.DeactivateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.GetQuestDefinitions;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;
using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Quests.IntegrationTests.PlayerQuests;

[TestFixture]
public sealed class QuestAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid SeedThreeGamesId = Guid.Parse("11111111-0000-0000-0000-000000000002");

    [Test]
    public async Task NonAdminPrincipal_Should_Be_RejectedWith403_ForAdminCommand()
    {
        AdminContext.Logout();

        var act = async () => await QuestsModule.ExecuteCommandAsync(
            new DeactivateQuestDefinitionCommand(SeedThreeGamesId));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task UpdateQuestDefinition_AsAdmin_ChangesGoal_AndWritesAuditRow()
    {
        AdminContext.LoginAs(AdminId);

        await QuestsModule.ExecuteCommandAsync(new UpdateQuestDefinitionCommand(
            SeedThreeGamesId, goal: 7, rewardAmount: 9, prerequisiteQuestType: null));

        var row = await QuerySingleOrDefaultAsync<QuestDefinitionRow>("""
            SELECT "Id", "Goal", "RewardAmount", "IsActive"
            FROM "quests"."QuestDefinitions" WHERE "Id" = @Id
            """, new { Id = SeedThreeGamesId });
        row.Should().NotBeNull();
        row!.Goal.Should().Be(7);
        row.RewardAmount.Should().Be(9);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "TargetId" = @TargetId
            ORDER BY "OccurredOn" DESC LIMIT 1
            """, new { TargetId = SeedThreeGamesId.ToString() });
        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.ActionType.Should().Be(nameof(UpdateQuestDefinitionCommand));
        audit.TargetType.Should().Be("Quests.QuestDefinition");
        audit.TargetId.Should().Be(SeedThreeGamesId.ToString());
    }

    [Test]
    public async Task DeactivateQuestDefinition_AsAdmin_FlipsIsActive_AndAudits()
    {
        AdminContext.LoginAs(AdminId);

        await QuestsModule.ExecuteCommandAsync(new DeactivateQuestDefinitionCommand(SeedThreeGamesId));

        var isActive = await QuerySingleOrDefaultAsync<bool>("""
            SELECT "IsActive" FROM "quests"."QuestDefinitions" WHERE "Id" = @Id
            """, new { Id = SeedThreeGamesId });
        isActive.Should().BeFalse();

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
            """,
            new { ActionType = nameof(DeactivateQuestDefinitionCommand), TargetId = SeedThreeGamesId.ToString() });
        auditCount.Should().Be(1);
    }

    [Test]
    public async Task CreateQuestDefinition_DuplicateType_Returns400_NoAuditRow()
    {
        AdminContext.LoginAs(AdminId);

        // ThreeGamesCompleted already seeded — duplicate Create must fail.
        var act = async () => await QuestsModule.ExecuteCommandAsync(
            new CreateQuestDefinitionCommand(
                QuestType.ThreeGamesCompleted, QuestCadence.OneTime, 3, 5, null));

        await act.Should().ThrowAsync<Common.Application.Exceptions.InvalidCommandException>();

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType
            """, new { ActionType = nameof(CreateQuestDefinitionCommand) });
        auditCount.Should().Be(0,
            "InvalidCommandException prevents the inner handler from completing, so no audit row is queued");
    }

    [Test]
    public async Task GetQuestDefinitions_AsAdmin_Returns_AllRows_IncludingInactive()
    {
        AdminContext.LoginAs(AdminId);
        await QuestsModule.ExecuteCommandAsync(new DeactivateQuestDefinitionCommand(SeedThreeGamesId));

        var definitions = await QuestsModule.ExecuteQueryAsync(new GetQuestDefinitionsQuery());

        definitions.Should().HaveCount(4);
        definitions.Should().Contain(d => d.Id == SeedThreeGamesId && d.IsActive == false);
    }

    [Test]
    public async Task IssueQuestToPlayer_AsAdmin_CreatesPlayerQuest_AndAuditsWithPlayerId()
    {
        AdminContext.LoginAs(AdminId);
        var playerId = Guid.NewGuid();

        await QuestsModule.ExecuteCommandAsync(
            new IssueQuestToPlayerCommand(playerId, QuestType.FirstGameCompleted));

        var pqCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "quests"."PlayerQuests" WHERE "PlayerId" = @PlayerId
            """, new { PlayerId = playerId });
        pqCount.Should().Be(1);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType
            """, new { ActionType = nameof(IssueQuestToPlayerCommand) });
        audit.Should().NotBeNull();
        audit!.TargetType.Should().Be("Quests.PlayerQuest");
        audit.TargetId.Should().Be(playerId.ToString());
    }

    private sealed record QuestDefinitionRow(Guid Id, int Goal, int RewardAmount, bool IsActive);
    private sealed record AdminActionRow(Guid AdminUserId, string ActionType, string TargetType, string? TargetId);
}
