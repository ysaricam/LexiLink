using LexiLink.Common.Domain;

namespace LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

public class QuestDescriptionMustNotExceedMaxLengthRule : IBusinessRule
{
    public const int MaxLength = 256;

    private readonly string? _description;

    public QuestDescriptionMustNotExceedMaxLengthRule(string? description)
    {
        _description = description;
    }

    public bool IsBroken() => (_description?.Length ?? 0) > MaxLength;

    public string Message => $"Quest description must not exceed {MaxLength} characters.";
}
