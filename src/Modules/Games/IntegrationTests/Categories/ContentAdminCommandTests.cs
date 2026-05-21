using LexiLink.Common.Application.Admin;
using LexiLink.Modules.Games.Application.Categories.CreateCategory;
using LexiLink.Modules.Games.Application.Categories.EditCategory;
using LexiLink.Modules.Games.Application.Links.ActivateLink;
using LexiLink.Modules.Games.Application.Links.CreateLink;
using LexiLink.Modules.Games.Application.Links.DeactivateLink;
using LexiLink.Modules.Games.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Games.IntegrationTests.Categories;

[TestFixture]
public sealed class ContentAdminCommandTests : TestBase
{
    private static readonly Guid AdminId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");

    [Test]
    public async Task NonAdmin_Should_Be_RejectedWith_AdminAuthorizationException()
    {
        AdminContext.Logout();

        var act = async () => await ExecuteCommandAsync(new CreateCategoryCommand("X", "desc"));

        await act.Should().ThrowAsync<AdminAuthorizationException>();
    }

    [Test]
    public async Task CreateCategory_AsAdmin_WritesAuditRow()
    {
        AdminContext.LoginAs(AdminId);

        var categoryId = await ExecuteCommandAsync(new CreateCategoryCommand("Spor", "test category"));

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>(
            """
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType
            ORDER BY "OccurredOn" DESC LIMIT 1
            """,
            new { ActionType = nameof(CreateCategoryCommand) });
        audit.Should().NotBeNull();
        audit!.AdminUserId.Should().Be(AdminId);
        audit.TargetType.Should().Be("Games.Category");
        // Create commands leave TargetId null; the freshly-allocated id is
        // in the JSON payload (verified by the EditCategory test below).
        audit.TargetId.Should().BeNull();

        // sanity: row landed in the categories table too
        var dbRow = await QuerySingleOrDefaultAsync<string>(
            """SELECT "Name" FROM "games"."Categories" WHERE "Id" = @Id""",
            new { Id = categoryId });
        dbRow.Should().Be("Spor");
    }

    [Test]
    public async Task EditCategory_AsAdmin_AuditTargetIdMatchesCategoryId()
    {
        AdminContext.LoginAs(AdminId);

        var categoryId = await ExecuteCommandAsync(new CreateCategoryCommand("Doğa", "init"));
        await ExecuteCommandAsync(new EditCategoryCommand(categoryId, "Doğa", "edited"));

        await ProcessOutboxAsync();

        var audit = await QuerySingleOrDefaultAsync<AdminActionRow>(
            """
            SELECT "AdminUserId", "ActionType", "TargetType", "TargetId"
            FROM "administration"."AdminActionAudit"
            WHERE "ActionType" = @ActionType
            """,
            new { ActionType = nameof(EditCategoryCommand) });
        audit.Should().NotBeNull();
        audit!.TargetType.Should().Be("Games.Category");
        audit.TargetId.Should().Be(categoryId.ToString());
    }

    [Test]
    public async Task CreateLink_AndDeactivate_FlipsActive_AndAuditsBoth()
    {
        AdminContext.LoginAs(AdminId);
        var categoryId = await ExecuteCommandAsync(new CreateCategoryCommand("Yemek", "category"));
        var linkId = await ExecuteCommandAsync(new CreateLinkCommand(categoryId, "elma", "fruit", true));

        await ExecuteCommandAsync(new DeactivateLinkCommand(linkId));
        var isActive = await QuerySingleOrDefaultAsync<bool>(
            """SELECT "IsActive" FROM "games"."Links" WHERE "Id" = @Id""",
            new { Id = linkId });
        isActive.Should().BeFalse();

        await ExecuteCommandAsync(new ActivateLinkCommand(linkId));
        isActive = await QuerySingleOrDefaultAsync<bool>(
            """SELECT "IsActive" FROM "games"."Links" WHERE "Id" = @Id""",
            new { Id = linkId });
        isActive.Should().BeTrue();

        await ProcessOutboxAsync();

        var auditCount = await QuerySingleOrDefaultAsync<int>(
            """
            SELECT COUNT(*)::int FROM "administration"."AdminActionAudit"
            WHERE "TargetType" = 'Games.Link' AND "TargetId" = @TargetId
            """,
            new { TargetId = linkId.ToString() });
        auditCount.Should().Be(2, "DeactivateLink + ActivateLink both audited against the same link id");
    }

    private sealed record AdminActionRow(Guid AdminUserId, string ActionType, string TargetType, string? TargetId);
}
