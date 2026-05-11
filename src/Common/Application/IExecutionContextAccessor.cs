namespace LexiLink.Common.Application;

public interface IExecutionContextAccessor
{
    Guid UserId { get; }

    Guid CorrelationId { get; }

    bool IsAvailable { get; }
}
