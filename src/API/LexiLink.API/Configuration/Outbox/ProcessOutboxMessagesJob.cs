using LexiLink.Common.Infrastructure.Outbox;
using Quartz;

namespace LexiLink.API.Configuration.Outbox;

[DisallowConcurrentExecution]
public sealed class ProcessOutboxMessagesJob : IJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var backgroundJob = context.JobDetail.Key.Name;
        var correlationId = Guid.NewGuid();
        using var jobLogScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["BackgroundJob"] = backgroundJob,
            ["QuartzFireInstanceId"] = context.FireInstanceId,
            ["QuartzTrigger"] = context.Trigger.Key.Name
        });

        _logger.LogInformation(
            "Background job {BackgroundJob} started. CorrelationId {CorrelationId}.",
            backgroundJob,
            correlationId);

        using var scope = _serviceScopeFactory.CreateScope();
        var processors = scope.ServiceProvider.GetServices<IOutboxProcessor>();

        foreach (var processor in processors)
        {
            var processorQueue = GetProcessorQueue(processor);
            using var processorLogScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["ProcessorQueue"] = processorQueue,
                ["ProcessorType"] = processor.GetType().FullName ?? processor.GetType().Name
            });

            try
            {
                _logger.LogInformation(
                    "Outbox processor {ProcessorQueue} started during background job {BackgroundJob}.",
                    processorQueue,
                    backgroundJob);

                await processor.ProcessAsync(context.CancellationToken);

                _logger.LogInformation(
                    "Outbox processor {ProcessorQueue} completed during background job {BackgroundJob}.",
                    processorQueue,
                    backgroundJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Outbox processor {ProcessorQueue} failed during background job {BackgroundJob}. CorrelationId {CorrelationId}.",
                    processorQueue,
                    backgroundJob,
                    correlationId);
            }
        }

        _logger.LogInformation(
            "Background job {BackgroundJob} completed. CorrelationId {CorrelationId}.",
            backgroundJob,
            correlationId);
    }

    private static string GetProcessorQueue(IOutboxProcessor processor)
    {
        if (processor is OutboxProcessor outboxProcessor)
        {
            return $"{outboxProcessor.SchemaName}-outbox";
        }

        return processor.GetType().Name;
    }
}
