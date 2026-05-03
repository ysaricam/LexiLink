using LexiLink.Modules.Games.Application.Configuration.Commands;
using LexiLink.Modules.Games.Application.Contracts;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace LexiLink.Modules.Games.Infrastructure.Configuration.Processing;

internal class LoggingCommandHandlerDecorator<T> : ICommandHandler<T>
    where T : ICommand
{
    private readonly ILogger _logger;
    private readonly ICommandHandler<T> _decorated;

    public LoggingCommandHandlerDecorator(
        ILogger logger,
        ICommandHandler<T> decorated)
    {
        _logger = logger;
        _decorated = decorated;
    }

    public async Task Handle(T command, CancellationToken cancellationToken)
    {
        using (LogContext.Push(new CommandLogEnricher(command)))
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
}
