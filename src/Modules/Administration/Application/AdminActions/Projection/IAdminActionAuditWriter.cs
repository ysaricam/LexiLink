using LexiLink.Modules.Administration.IntegrationEvents;

namespace LexiLink.Modules.Administration.Application.AdminActions.Projection;

/// <summary>
/// Persists <see cref="AdminActionPerformedIntegrationEvent"/> into the
/// audit projection table. Idempotent on event id — re-publishing the
/// same event is a no-op. The implementation lives in
/// Administration.Infrastructure with raw Dapper, matching the
/// projection-only style used by Stats.
/// </summary>
public interface IAdminActionAuditWriter
{
    Task AppendAsync(AdminActionPerformedIntegrationEvent @event, CancellationToken cancellationToken);
}
