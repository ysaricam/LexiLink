using LexiLink.Common.Application;

namespace LexiLink.Modules.Players.IntegrationTests.SeedWork;

internal sealed class TestExecutionContextAccessor : IExecutionContextAccessor
{
    public Guid UserId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public bool IsAvailable => true;
}
