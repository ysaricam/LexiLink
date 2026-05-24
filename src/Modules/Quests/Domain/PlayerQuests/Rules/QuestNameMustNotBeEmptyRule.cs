using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestNameMustNotBeEmptyRule : IBusinessRule
{
    private readonly string? _name;

    public QuestNameMustNotBeEmptyRule(string? name)
    {
        _name = name;
    }

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Quest name must not be empty.";
}
