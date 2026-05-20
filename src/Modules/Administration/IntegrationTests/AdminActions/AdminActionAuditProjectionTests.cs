using Autofac;
using LexiLink.Modules.Administration.Application.AdminActions.GetAdminActions;
using LexiLink.Modules.Administration.Application.Contracts;
using LexiLink.Modules.Administration.IntegrationEvents;
using LexiLink.Modules.Administration.IntegrationTests.SeedWork;

namespace LexiLink.Modules.Administration.IntegrationTests.AdminActions;

[TestFixture]
public sealed class AdminActionAuditProjectionTests : TestBase
{
    [Test]
    public async Task AdminActionPerformedIntegrationEvent_Should_ProjectIntoAuditTable()
    {
        var adminId = Guid.NewGuid();
        var evt = NewEvent(adminId, "Quests.CreateQuestDefinitionCommand",
            targetType: "Quests.QuestDefinition",
            targetId: Guid.NewGuid().ToString(),
            payload: "{\"goal\":3,\"reward\":5}");

        await EventsBus.PublishAsync(evt);

        var module = Scope.Resolve<IAdministrationModule>();
        var actions = await module.ExecuteQueryAsync(
            new GetAdminActionsQuery(adminUserId: adminId));

        actions.Should().HaveCount(1);
        actions[0].Id.Should().Be(evt.Id);
        actions[0].AdminUserId.Should().Be(adminId);
        actions[0].ActionType.Should().Be("Quests.CreateQuestDefinitionCommand");
        actions[0].TargetType.Should().Be("Quests.QuestDefinition");
        actions[0].TargetId.Should().Be(evt.TargetId);
        actions[0].PayloadJson.Should().Be("{\"goal\":3,\"reward\":5}");
    }

    [Test]
    public async Task RepublishingSameEventId_Should_BeIdempotent()
    {
        var adminId = Guid.NewGuid();
        var evt = NewEvent(adminId, "Energy.SetPlayerEnergyCommand",
            targetType: "Energy.PlayerEnergy",
            targetId: Guid.NewGuid().ToString(),
            payload: "{\"amount\":5}");

        await EventsBus.PublishAsync(evt);
        await EventsBus.PublishAsync(evt);

        var module = Scope.Resolve<IAdministrationModule>();
        var actions = await module.ExecuteQueryAsync(
            new GetAdminActionsQuery(adminUserId: adminId));

        actions.Should().HaveCount(1, "AppendAsync uses INSERT ON CONFLICT (Id) DO NOTHING");
    }

    [Test]
    public async Task Query_Should_FilterByTargetTypeAndId()
    {
        var actor = Guid.NewGuid();
        var targetA = Guid.NewGuid().ToString();
        var targetB = Guid.NewGuid().ToString();

        await EventsBus.PublishAsync(NewEvent(actor, "X", "Players.Player", targetA, "{}"));
        await EventsBus.PublishAsync(NewEvent(actor, "X", "Players.Player", targetB, "{}"));
        await EventsBus.PublishAsync(NewEvent(actor, "X", "Games.Category", targetA, "{}"));

        var module = Scope.Resolve<IAdministrationModule>();

        var byTargetA = await module.ExecuteQueryAsync(
            new GetAdminActionsQuery(targetType: "Players.Player", targetId: targetA));
        byTargetA.Should().HaveCount(1);
        byTargetA[0].TargetId.Should().Be(targetA);
        byTargetA[0].TargetType.Should().Be("Players.Player");

        var allPlayers = await module.ExecuteQueryAsync(
            new GetAdminActionsQuery(targetType: "Players.Player"));
        allPlayers.Should().HaveCount(2);
    }

    [Test]
    public async Task Query_Should_OrderByOccurredOn_Descending()
    {
        var adminId = Guid.NewGuid();
        var earliest = NewEvent(adminId, "A", "T", null, "{}", occurredOn: DateTime.UtcNow.AddMinutes(-10));
        var middle = NewEvent(adminId, "B", "T", null, "{}", occurredOn: DateTime.UtcNow.AddMinutes(-5));
        var newest = NewEvent(adminId, "C", "T", null, "{}", occurredOn: DateTime.UtcNow);

        // Publish in non-chronological order
        await EventsBus.PublishAsync(middle);
        await EventsBus.PublishAsync(newest);
        await EventsBus.PublishAsync(earliest);

        var module = Scope.Resolve<IAdministrationModule>();
        var actions = await module.ExecuteQueryAsync(
            new GetAdminActionsQuery(adminUserId: adminId));

        actions.Select(a => a.ActionType).Should().ContainInOrder("C", "B", "A");
    }

    private static AdminActionPerformedIntegrationEvent NewEvent(
        Guid adminUserId,
        string actionType,
        string targetType,
        string? targetId,
        string payload,
        DateTime? occurredOn = null) =>
        new(
            Id: Guid.NewGuid(),
            OccurredOn: occurredOn ?? DateTime.UtcNow,
            AdminUserId: adminUserId,
            ActionType: actionType,
            TargetType: targetType,
            TargetId: targetId,
            PayloadJson: payload);
}
