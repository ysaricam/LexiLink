using LexiLink.Common.Application;
using LexiLink.Modules.Players.Application.Configuration.Commands;
using LexiLink.Modules.Players.Application.Contracts;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace LexiLink.Modules.Players.Infrastructure.Configuration.Processing;

internal class LoggingCommandHandlerDecorator<T> : ICommandHandler<T>
    where T : ICommand
{
    private readonly ILogger _logger;
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly ICommandHandler<T> _decorated;

    public LoggingCommandHandlerDecorator(
        ILogger logger,
        IExecutionContextAccessor executionContextAccessor,
        ICommandHandler<T> decorated)
    {
        _logger = logger;
        _executionContextAccessor = executionContextAccessor;
        _decorated = decorated;
    }

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        using (LogContext.Push(
            new RequestLogEnricher(_executionContextAccessor),
            new CommandLogEnricher(command)))
        {
            try
            {
                _logger.Information(
                    "Executing command {Command}",
                    command.GetType().Name);

                await _decorated.Handle(command, cancellationToken);

                _logger.Information(
                    "Command {Command} processed successful",
                    command.GetType().Name);
            }
            catch (Exception exception)
            {
                _logger.Error(
                    exception,
                    "Command {Command} processing failed",
                    command.GetType().Name);
                throw;
            }
        }
    }

    private class CommandLogEnricher : ILogEventEnricher
    {
        private readonly ICommand _command;

        public CommandLogEnricher(ICommand command)
        {
            _command = command;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty(
                "Context",
                new ScalarValue($"Command:{_command.Id}")));
        }
    }

    private class RequestLogEnricher : ILogEventEnricher
    {
        private readonly IExecutionContextAccessor _executionContextAccessor;

        public RequestLogEnricher(IExecutionContextAccessor executionContextAccessor)
        {
            _executionContextAccessor = executionContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!_executionContextAccessor.IsAvailable)
            {
                return;
            }

            try
            {
                var correlationId = _executionContextAccessor.CorrelationId;
                logEvent.AddOrUpdateProperty(new LogEventProperty(
                    "CorrelationId",
                    new ScalarValue(correlationId)));
            }
            catch (ApplicationException)
            {
                // execution context not yet wired (no auth/correlation) — silently skip
            }
        }
    }
}
