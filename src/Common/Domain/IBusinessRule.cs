namespace LexiLink.Common.Domain;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
}
