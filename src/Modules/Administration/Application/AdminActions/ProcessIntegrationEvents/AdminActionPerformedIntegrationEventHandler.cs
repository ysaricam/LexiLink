using LexiLink.Common.Application.IntegrationEvents;
using LexiLink.Modules.Administration.Application.AdminActions.Projection;
using LexiLink.Modules.Administration.IntegrationEvents;

namespace LexiLink.Modules.Administration.Application.AdminActions.ProcessIntegrationEvents;

internal sealed class AdminActionPerformedIntegrationEventHandler
    : IIntegrationEventHandler<AdminActionPerformedIntegrationEvent>
{
    private readonly IAdminActionAuditWriter _writer;

    internal AdminActionPerformedIntegrationEventHandler(IAdminActionAuditWriter writer)
    {
        _writer = writer;
    }

    public Task Handle(
        AdminActionPerformedIntegrationEvent @event,
        CancellationToken cancellationToken = default) =>
        _writer.AppendAsync(@event, cancellationToken);
}
