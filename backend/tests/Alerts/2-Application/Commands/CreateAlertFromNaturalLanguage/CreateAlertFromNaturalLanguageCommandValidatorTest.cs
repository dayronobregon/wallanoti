using FluentValidation.TestHelper;
using Wallanoti.Src.Alerts.Application.Commands.CreateAlertFromNaturalLanguage;

namespace Wallanoti.Tests.Alerts._2_Application.Commands.CreateAlertFromNaturalLanguage;

public class CreateAlertFromNaturalLanguageCommandValidatorTest
{
    private readonly CreateAlertFromNaturalLanguageCommandValidator _validator;

    public CreateAlertFromNaturalLanguageCommandValidatorTest()
    {
        _validator = new CreateAlertFromNaturalLanguageCommandValidator();
    }

    [Fact]
    public void Validate_EmptyQuery_ReturnsError()
    {
        // Arrange
        var command = new CreateAlertFromNaturalLanguageCommand(1, string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NaturalLanguageQuery);
    }

    [Fact]
    public void Validate_WhitespaceQuery_ReturnsError()
    {
        // Arrange
        var command = new CreateAlertFromNaturalLanguageCommand(1, "   ");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NaturalLanguageQuery);
    }

    [Fact]
    public void Validate_TooShortQuery_ReturnsError()
    {
        // Arrange - query with 2 characters is below minimum of 3
        var command = new CreateAlertFromNaturalLanguageCommand(1, "ab");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NaturalLanguageQuery)
            .WithErrorMessage("Query must be at least 3 characters");
    }

    [Fact]
    public void Validate_ValidQuery_DoesNotReturnError()
    {
        // Arrange
        var command = new CreateAlertFromNaturalLanguageCommand(1, "iphone 14 pro max");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NaturalLanguageQuery);
    }

    [Fact]
    public void Validate_ExactlyThreeCharacters_DoesNotReturnError()
    {
        // Arrange - minimum length boundary
        var command = new CreateAlertFromNaturalLanguageCommand(1, "abc");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NaturalLanguageQuery);
    }
}
