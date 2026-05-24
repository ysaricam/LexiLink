using FluentValidation;
using LexiLink.Modules.Quests.Domain.PlayerQuests.Rules;

namespace LexiLink.Modules.Quests.Application.Admin.QuestDefinitions.UpdateQuestDefinition;

internal sealed class UpdateQuestDefinitionCommandValidator : AbstractValidator<UpdateQuestDefinitionCommand>
{
    public UpdateQuestDefinitionCommandValidator()
    {
        RuleFor(x => x.QuestDefinitionId).NotEmpty();
        RuleFor(x => x.Description).NotNull().MaximumLength(QuestDescriptionMustNotExceedMaxLengthRule.MaxLength);
        RuleFor(x => x.ProgressBaseline).IsInEnum();
        RuleFor(x => x.Threshold).GreaterThan(0);
        RuleFor(x => x.Reward).GreaterThan(0);
    }
}
