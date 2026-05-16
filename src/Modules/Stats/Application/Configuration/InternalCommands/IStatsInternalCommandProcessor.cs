namespace LexiLink.Modules.Stats.Application.Configuration.InternalCommands;

public interface IStatsInternalCommandProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken = default);
}
