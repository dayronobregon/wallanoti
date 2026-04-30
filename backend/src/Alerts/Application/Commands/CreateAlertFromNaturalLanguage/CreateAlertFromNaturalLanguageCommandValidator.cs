using FluentValidation;

namespace Wallanoti.Src.Alerts.Application.Commands.CreateAlertFromNaturalLanguage;

/// <summary>
/// Validator for CreateAlertFromNaturalLanguageCommand.
/// Rules:
/// - NaturalLanguageQuery: required, minimum 3 characters
/// </summary>
public sealed class CreateAlertFromNaturalLanguageCommandValidator
    : AbstractValidator<CreateAlertFromNaturalLanguageCommand>
{
    public CreateAlertFromNaturalLanguageCommandValidator()
    {
        RuleFor(x => x.NaturalLanguageQuery)
            .NotEmpty()
            .WithMessage("Query must be at least 3 characters");
        
        RuleFor(x => x.NaturalLanguageQuery)
            .MinimumLength(3)
            .WithMessage("Query must be at least 3 characters");
    }
}
