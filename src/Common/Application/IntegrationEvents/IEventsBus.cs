namespace LexiLink.Common.Application.IntegrationEvents;

public interface IEventsBus
{
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
