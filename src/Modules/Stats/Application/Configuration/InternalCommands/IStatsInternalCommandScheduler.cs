namespace LexiLink.Modules.Stats.Application.Configuration.InternalCommands;

public interface IStatsInternalCommandScheduler
{
    Task ScheduleAsync(
        IInternalCommand command,
        DateTime? dueDate = null,
        CancellationToken cancellationToken = default);
}
