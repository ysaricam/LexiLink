using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestNameMustNotExceedMaxLengthRule : IBusinessRule
{
    public const int MaxLength = 64;

    private readonly string? _name;

    public QuestNameMustNotExceedMaxLengthRule(string? name)
    {
        _name = name;
    }

    public bool IsBroken() => (_name?.Length ?? 0) > MaxLength;

    public string Message => $"Quest name must not exceed {MaxLength} characters.";
}
