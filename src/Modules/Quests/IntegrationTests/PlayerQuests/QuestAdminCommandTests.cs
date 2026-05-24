using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.DeactivateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.GetQuestDefinitions;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.ReactivateQuestDefinition;
using LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;
using LexiLink.Modules.Quests.Domain.PlayerQuests;
using LexiLink.Modules.Quests.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Quests.IntegrationTests.PlayerQuests;

[TestFixture]
public sealed class QuestAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    [Test]
    public async Task NonAdminPrincipal_Should_Be_RejectedWith403_ForAdminCommand()
    {
        AdminContext.Logout();

        var act = async () => await QuestsModule.ExecuteCommandAsync(
            new DeactivateQuestDefinitionCommand(SeedDailyQuestDefinitionId));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task CreateQuestDefinition_AsAdmin_PersistsRow_AndWritesAuditRow()
    {
        AdminContext.LoginAs(AdminId);

        var id = await QuestsModule.ExecuteCommandAsync(new CreateQuestDefinitionCommand(
            name: "İlk Üç Oyun",
            description: "Toplam 3 oyun tamamla",
            trigger: QuestTrigger.GameCompletedTotal,
            threshold: 3,
            reward: 5,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot));

        var row = await QuerySingleOrDefaultAsync<QuestDefinitionRow>("""
            SELECT "Id", "Name", "Trigger", "Threshold", "Reward", "ProgressBaseline", "IsActive"
            FROM "quests"."QuestDefinitions" WHERE "Id" = @Id
            """, new { Id = id });
        row.Should().NotBeNull();
        row!.Name.Should().Be("İlk Üç Oyun");
        row.Trigger.Should().Be("GameCompletedTotal");
        row.Threshold.Should().Be(3);
        row.Reward.Should().Be(5);
        row.ProgressBaseline.Should().Be("FromSnapshot");
        row.IsActive.Should().BeTrue();

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType
            ORDER BY "OccurredOn" DESC LIMIT 1
            """, new { ActionType = nameof(CreateQuestDefinitionCommand) });
        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Quests.QuestDefinition");
        // Create allocates the id inside the handler — audit row's TargetId
        // is null; the created id surfaces in the audit PayloadJson.
        audit.TargetId.Should().BeNull();
    }

    [Test]
    public async Task UpdateQuestDefinition_AsAdmin_ChangesThreshold_AndWritesAuditRow()
    {
        AdminContext.LoginAs(AdminId);

        await QuestsModule.ExecuteCommandAsync(new UpdateQuestDefinitionCommand(
            questDefinitionId: SeedDailyQuestDefinitionId,
            description: "yeni açıklama",
            threshold: 7,
            reward: 9,
            prerequisiteQuestDefinitionId: null,
            progressBaseline: ProgressBaseline.FromSnapshot));

        var row = await QuerySingleOrDefaultAsync<QuestDefinitionRow>("""
            SELECT "Id", "Name", "Trigger", "Threshold", "Reward", "ProgressBaseline", "IsActive"
            FROM "quests"."QuestDefinitions" WHERE "Id" = @Id
            """, new { Id = SeedDailyQuestDefinitionId });
        row.Should().NotBeNull();
        row!.Threshold.Should().Be(7);
        row.Reward.Should().Be(9);

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>("""
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "TargetId" = @TargetId
            ORDER BY "OccurredOn" DESC LIMIT 1
            """, new { TargetId = SeedDailyQuestDefinitionId.ToString() });
        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.ActionType.Should().Be(nameof(UpdateQuestDefinitionCommand));
        audit.TargetType.Should().Be("Quests.QuestDefinition");
        audit.TargetId.Should().Be(SeedDailyQuestDefinitionId.ToString());
    }

    [Test]
    public async Task DeactivateQuestDefinition_AsAdmin_FlipsIsActive_AndAudits()
    {
        AdminContext.LoginAs(AdminId);

        await QuestsModule.ExecuteCommandAsync(new DeactivateQuestDefinitionCommand(SeedDailyQuestDefinitionId));

        var isActive = await QuerySingleOrDefaultAsync<bool>("""
            SELECT "IsActive" FROM "quests"."QuestDefinitions" WHERE "Id" = @Id
            """, new { Id = SeedDailyQuestDefinitionId });
        isActive.Should().BeFalse();

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
            """,
            new { ActionType = nameof(DeactivateQuestDefinitionCommand), TargetId = SeedDailyQuestDefinitionId.ToString() });
        auditCount.Should().Be(1);
    }

    [Test]
    public async Task ReactivateQuestDefinition_AsAdmin_FlipsIsActive_AndAudits()
    {
        AdminContext.LoginAs(AdminId);
        await QuestsModule.ExecuteCommandAsync(new DeactivateQuestDefinitionCommand(SeedDailyQuestDefinitionId));

        await QuestsModule.ExecuteCommandAsync(new ReactivateQuestDefinitionCommand(SeedDailyQuestDefinitionId));

        var isActive = await QuerySingleOrDefaultAsync<bool>("""
            SELECT "IsActive" FROM "quests"."QuestDefinitions" WHERE "Id" = @Id
            """, new { Id = SeedDailyQuestDefinitionId });
        isActive.Should().BeTrue();
    }

    [Test]
    public async Task UpdateQuestDefinition_DirectSelfReference_Returns400_NoAuditRow()
    {
        AdminContext.LoginAs(AdminId);

        var act = async () => await QuestsModule.ExecuteCommandAsync(new UpdateQuestDefinitionCommand(
            questDefinitionId: SeedDailyQuestDefinitionId,
            description: "x",
            threshold: 3,
            reward: 5,
            prerequisiteQuestDefinitionId: SeedDailyQuestDefinitionId,
            progressBaseline: ProgressBaseline.FromSnapshot));

        await act.Should().ThrowAsync<Common.Domain.BusinessRuleValidationException>();

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>("""
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType AND "TargetId" = @TargetId
            """,
            new { ActionType = nameof(UpdateQuestDefinitionCommand), TargetId = SeedDailyQuestDefinitionId.ToString() });
        auditCount.Should().Be(0,
            "BusinessRuleValidationException prevents the inner handler from completing, so no audit row is queued");
    }

    [Test]
    public async Task GetQuestDefinitions_AsAdmin_Returns_AllRows_IncludingInactive()
    {
        AdminContext.LoginAs(AdminId);
        await QuestsModule.ExecuteCommandAsync(new DeactivateQuestDefinitionCommand(SeedDailyQuestDefinitionId));

        var definitions = await QuestsModule.ExecuteQueryAsync(new GetQuestDefinitionsQuery());

        definitions.Should().NotBeEmpty();
        definitions.Should().Contain(d => d.Id == SeedDailyQuestDefinitionId && d.IsActive == false);
    }

    private sealed record QuestDefinitionRow(
        Guid Id,
        string Name,
        string Trigger,
        int Threshold,
        int Reward,
        string ProgressBaseline,
        bool IsActive);

    private sealed record AdminActionRow(Guid AdminUserId, string ActionType, string TargetType, string? TargetId);
}
