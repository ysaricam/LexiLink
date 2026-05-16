using LexiLink.Modules.Stats.Application.Configuration.InternalCommands;
using LexiLink.Modules.Stats.Application.PlayerStats.ProcessIntegrationEvents;
using Quartz;

namespace LexiLink.API.Configuration.Inbox;

[DisallowConcurrentExecution]
public sealed class ProcessStatsInboxMessagesJob : IJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProcessStatsInboxMessagesJob> _logger;

    public ProcessStatsInboxMessagesJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessStatsInboxMessagesJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var backgroundJob = context.JobDetail.Key.Name;
        var correlationId = Guid.NewGuid();
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["BackgroundJob"] = backgroundJob,
            ["ProcessorQueue"] = "stats-internal-commands",
            ["QuartzFireInstanceId"] = context.FireInstanceId,
            ["QuartzTrigger"] = context.Trigger.Key.Name
        });

        _logger.LogInformation(
            "Background job {BackgroundJob} started for processor queue {ProcessorQueue}. CorrelationId {CorrelationId}.",
            backgroundJob,
            "stats-internal-commands",
            correlationId);

        using var scope = _serviceScopeFactory.CreateScope();
        var scheduler = scope.ServiceProvider.GetRequiredService<IStatsInternalCommandScheduler>();
        var processor = scope.ServiceProvider.GetRequiredService<IStatsInternalCommandProcessor>();

        try
        {
            _logger.LogInformation(
                "Scheduling Stats inbox processing internal command during background job {BackgroundJob}.",
                backgroundJob);

            await scheduler.ScheduleAsync(new ProcessStatsInboxCommand(), cancellationToken: context.CancellationToken);

            _logger.LogInformation(
                "Processing Stats internal commands during background job {BackgroundJob}.",
                backgroundJob);

            await processor.ProcessAsync(context.CancellationToken);

            _logger.LogInformation(
                "Background job {BackgroundJob} completed for processor queue {ProcessorQueue}. CorrelationId {CorrelationId}.",
                backgroundJob,
                "stats-internal-commands",
                correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Stats inbox internal command scheduling or processing failed during background job {BackgroundJob}. CorrelationId {CorrelationId}.",
                backgroundJob,
                correlationId);
        }
    }
}
