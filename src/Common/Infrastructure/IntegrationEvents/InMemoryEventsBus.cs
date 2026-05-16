using LexiLink.Common.Application.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;

namespace LexiLink.Common.Infrastructure.IntegrationEvents;

public sealed class InMemoryEventsBus : IEventsBus
{
    private readonly IServiceProvider _serviceProvider;

    public InMemoryEventsBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            var handleMethod = handler.GetType().GetMethod(
                nameof(IIntegrationEventHandler<IIntegrationEvent>.Handle),
                [integrationEvent.GetType(), typeof(CancellationToken)]);
            if (handleMethod is null)
            {
                throw new InvalidOperationException(
                    $"Integration event handler '{handler.GetType().FullName}' does not expose a compatible Handle method.");
            }

            var result = handleMethod.Invoke(handler, [integrationEvent, cancellationToken]);
            if (result is not Task task)
            {
                throw new InvalidOperationException(
                    $"Integration event handler '{handler.GetType().FullName}' Handle method did not return a Task.");
            }

            await task;
        }
    }
}
