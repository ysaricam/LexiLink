using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Contracts;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace LexiLink.Modules.Games.Infrastructure.Configuration.Processing;

internal class LoggingCommandHandlerWithResultDecorator<T, TResult> : ICommandHandler<T, TResult>
    where T : ICommand<TResult>
{
    private readonly ILogger _logger;
    private readonly ICommandHandler<T, TResult> _decorated;

    public LoggingCommandHandlerWithResultDecorator(
        ILogger logger,
        ICommandHandler<T, TResult> decorated)
    {
        _logger = logger;
        _decorated = decorated;
    }

    public async Task<TResult> Handle(T command, CancellationToken cancellationToken)
    {
        using (LogContext.Push(new CommandLogEnricher(command)))
        {
            try
            {
                _logger.Information(
                    "Executing command {@Command}",
                    command);

                var result = await _decorated.Handle(command, cancellationToken);

                _logger.Information(
                    "Command processed successful, result {Result}",
                    result);

                return result;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Command processing failed");
                throw;
            }
        }
    }

    private class CommandLogEnricher : ILogEventEnricher
    {
        private readonly ICommand<TResult> _command;

        public CommandLogEnricher(ICommand<TResult> command)
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
}
