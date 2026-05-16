using LexiLink.Modules.Stats.Application.Configuration.Commands;

namespace LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;

internal sealed class ProcessStatsInboxCommandHandler : ICommandHandler<ProcessStatsInboxCommand>
{
    private readonly IStatsInboxProcessor _statsInboxProcessor;

    internal ProcessStatsInboxCommandHandler(IStatsInboxProcessor statsInboxProcessor)
    {
        _statsInboxProcessor = statsInboxProcessor;
    }

    public Task Handle(ProcessStatsInboxCommand request, CancellationToken cancellationToken) =>
        _statsInboxProcessor.ProcessAsync(cancellationToken);
}
