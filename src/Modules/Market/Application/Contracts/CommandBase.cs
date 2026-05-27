namespace LexiLink.Modules.Market.Application.Contracts;

public abstract class CommandBase : ICommand
{
    protected CommandBase()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }
}

public abstract class CommandBase<TResult> : ICommand<TResult>
{
    protected CommandBase()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }
}
