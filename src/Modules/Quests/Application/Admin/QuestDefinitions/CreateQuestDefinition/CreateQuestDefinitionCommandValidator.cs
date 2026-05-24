using FluentValidation;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.CreateQuestDefinition;

internal sealed class CreateQuestDefinitionCommandValidator : AbstractValidator<CreateQuestDefinitionCommand>
{
    public CreateQuestDefinitionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(QuestNameMustNotExceedMaxLengthRule.MaxLength);
        RuleFor(x => x.Description).NotNull().MaximumLength(QuestDescriptionMustNotExceedMaxLengthRule.MaxLength);
        RuleFor(x => x.Trigger).IsInEnum();
        RuleFor(x => x.ProgressBaseline).IsInEnum();
        RuleFor(x => x.Threshold).GreaterThan(0);
        RuleFor(x => x.Reward).GreaterThan(0);
    }
}
