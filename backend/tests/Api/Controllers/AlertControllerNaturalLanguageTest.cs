using System.Security.Claims;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Wallanoti.Api.Controllers;
using Wallanoti.Src.Alerts.Application.Commands.CreateAlertFromNaturalLanguage;
using Wallanoti.Src.Alerts.Domain.DTOs;
using Wallanoti.Src.Shared.Domain;

namespace Wallanoti.Tests.Api.Controllers;

public class AlertControllerNaturalLanguageTest
{
    private static (AlertController controller, Mock<IMediator> mediatorMock, UserContext userContext) CreateController(
        long userId = 12345L)
    {
        var userContext = new UserContext();
        // Set up the user context with bearer token, userId, and username
        userContext.SetUser("test-bearer-token", userId, "TestUser");

        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<AlertController>>();
        var controller = new AlertController(mediatorMock.Object, userContext);

        return (controller, mediatorMock, userContext);
    }

    [Fact]
    public async Task CreateFromNaturalLanguage_ValidRequest_Returns201Created()
    {
        // Arrange
        var (controller, mediatorMock, _) = CreateController();
        var request = new CreateAlertFromNaturalLanguageRequest("iphone 14 pro max, max 300€, Barcelona");
        var expectedResponse = new CreateAlertFromNaturalLanguageResponse(
            Id: Guid.NewGuid(),
            Name: "iphone 14 pro max, max 300€, Barcelona",
            Url: "https://es.wallapop.com/items?keywords=iphone+14+pro+max&max_price=300",
            IsActive: true,
            CreatedAt: DateTime.UtcNow);

        mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateAlertFromNaturalLanguageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await controller.CreateFromNaturalLanguage(request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        
        var alertResponse = Assert.IsType<CreateAlertFromNaturalLanguageResponse>(createdResult.Value);
        Assert.Equal(expectedResponse.Name, alertResponse.Name);
        Assert.Equal(expectedResponse.Url, alertResponse.Url);
        Assert.True(alertResponse.IsActive);
    }

    [Fact]
    public async Task CreateFromNaturalLanguage_ValidationFailure_Returns400BadRequest()
    {
        // Arrange
        var (controller, mediatorMock, _) = CreateController();
        var request = new CreateAlertFromNaturalLanguageRequest("ab"); // Too short, < 3 chars

        mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateAlertFromNaturalLanguageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException(new[]
            {
                new ValidationFailure(
                    "NaturalLanguageQuery",
                    "Query must be at least 3 characters")
            }));

        // Act
        var result = await controller.CreateFromNaturalLanguage(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateFromNaturalLanguage_ZeroKeywords_Returns422UnprocessableEntity()
    {
        // Arrange
        var (controller, mediatorMock, _) = CreateController();
        var request = new CreateAlertFromNaturalLanguageRequest("xyz123 nonsense");

        var emptyEntities = new NerEntities(
            Keywords: new List<string>(),
            Brand: null,
            MaxPrice: null,
            MinPrice: null,
            Category: null,
            Location: null
        );

        mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateAlertFromNaturalLanguageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NerExtractionFailedException(emptyEntities));

        // Act
        var result = await controller.CreateFromNaturalLanguage(request);

        // Assert
        var unprocessableResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessableResult.StatusCode);
        
        // Issue 3 verification: response should include extractedEntities (even if empty)
        var problemDetails = unprocessableResult.Value as ProblemDetails;
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails.Extensions.ContainsKey("extractedEntities"));
    }

    [Fact]
    public async Task CreateFromNaturalLanguage_NerTimeout_Returns503ServiceUnavailable()
    {
        // Arrange
        var (controller, mediatorMock, _) = CreateController();
        var request = new CreateAlertFromNaturalLanguageRequest("iphone 13");

        mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateAlertFromNaturalLanguageCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("NER service timed out"));

        // Act
        var result = await controller.CreateFromNaturalLanguage(request);

        // Assert
        var serviceUnavailableResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, serviceUnavailableResult.StatusCode);
        
        // Verify the error message indicates NER service unavailability
        var problemDetails = serviceUnavailableResult.Value as ProblemDetails;
        Assert.NotNull(problemDetails);
        Assert.Contains("NER service temporarily unavailable", problemDetails.Detail);
    }
}