using Serilog.Core;
using Serilog.Events;

namespace LexiLink.Modules.Players.IntegrationTests.SeedWork;

public sealed class CollectingLogEventSink : ILogEventSink
{
    private readonly object _gate = new();
    private readonly List<LogEvent> _events = [];

    public IReadOnlyList<LogEvent> Events
    {
        get
        {
            lock (_gate)
            {
                return _events.ToList();
            }
        }
    }

    public void Emit(LogEvent logEvent)
    {
        lock (_gate)
        {
            _events.Add(logEvent);
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _events.Clear();
        }
    }
}
