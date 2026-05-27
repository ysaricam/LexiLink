namespace LexiLink.Modules.Market.Application.Contracts;

public abstract class QueryBase<TResult> : IQuery<TResult>
{
    protected QueryBase()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }
}
