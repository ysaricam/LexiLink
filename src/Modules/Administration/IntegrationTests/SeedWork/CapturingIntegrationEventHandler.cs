using LexiLink.Common.Application.IntegrationEvents;

namespace LexiLink.Modules.Administration.IntegrationTests.SeedWork;

/// <summary>
/// Singleton test handler that captures every integration event of type
/// <typeparamref name="TEvent"/> dispatched by the in-memory event bus.
/// Registered into the test container so the outbox processor's publish
/// path can be asserted without scraping logs.
/// </summary>
public sealed class CapturingIntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
    where TEvent : IIntegrationEvent
{
    private readonly List<TEvent> _captured = [];
    private readonly object _gate = new();

    public IReadOnlyList<TEvent> Captured
    {
        get
        {
            lock (_gate)
            {
                return _captured.ToList();
            }
        }
    }

    public Task Handle(TEvent @event, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _captured.Add(@event);
        }
        return Task.CompletedTask;
    }

    public void Clear()
    {
        lock (_gate)
        {
            _captured.Clear();
        }
    }
}
