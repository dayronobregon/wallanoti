using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Wallanoti.Src.Alerts.Application.Commands.CreateAlertFromNaturalLanguage;
using Wallanoti.Src.Alerts.Application.CreateAlert;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.DTOs;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Alerts.Domain.Services;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Tests.Alerts._2_Application.Commands.CreateAlertFromNaturalLanguage;

public class CreateAlertFromNaturalLanguageCommandHandlerTest
{
    private readonly Mock<INerService> _nerServiceMock = new();
    private readonly Mock<IWallapopUrlBuilder> _urlBuilderMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new();
    private readonly Mock<ILogger<CreateAlertFromNaturalLanguageCommandHandler>> _loggerMock = new();
    private readonly CreateAlertFromNaturalLanguageCommandHandler _handler;

    public CreateAlertFromNaturalLanguageCommandHandlerTest()
    {
        _eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);
        _mediatorMock.Setup(x => x.Send(It.IsAny<CreateAlertCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Unit.Task);
        _handler = new CreateAlertFromNaturalLanguageCommandHandler(
            _nerServiceMock.Object,
            _urlBuilderMock.Object,
            _mediatorMock.Object,
            _alertRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_HappyPath_UsesMediatorToCreateAlert()
    {
        // This test verifies Issue 1 fix: handler should dispatch CreateAlertCommand via IMediator
        // Arrange
        var userId = 12345L;
        var query = "iphone 14 pro max, max 300€, Barcelona";
        var command = new CreateAlertFromNaturalLanguageCommand(userId, query);
        
        var nerEntities = new NerEntities(
            Keywords: new List<string> { "iphone", "14", "pro", "max" },
            Brand: "Apple",
            MaxPrice: 300,
            MinPrice: null,
            Category: "electronics",
            Location: "Barcelona"
        );
        
        var expectedUrl = "https://es.wallapop.com/items?keywords=iphone+14+pro+max&max_price=300&location=Barcelona";
        
        _nerServiceMock.Setup(x => x.ExtractEntitiesAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nerEntities);
        _urlBuilderMock.Setup(x => x.BuildUrl(nerEntities))
            .Returns(expectedUrl);
        
        // Setup repository to return a created alert
        var createdAlert = Alert.Create(userId, query, expectedUrl, DateTime.UtcNow);
        _alertRepositoryMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync(new List<Alert> { createdAlert });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateAlertCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_KeywordsOnly_NerExtractsOnlyKeywords_CreatesAlertWithMinimalUrl()
    {
        // Arrange
        var userId = 1L;
        var query = "gaming laptop";
        var command = new CreateAlertFromNaturalLanguageCommand(userId, query);
        
        var nerEntities = new NerEntities(
            Keywords: new List<string> { "gaming", "laptop" },
            Brand: null,
            MaxPrice: null,
            MinPrice: null,
            Category: null,
            Location: null
        );
        
        var expectedUrl = "https://es.wallapop.com/items?keywords=gaming+laptop";
        
        _nerServiceMock.Setup(x => x.ExtractEntitiesAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nerEntities);
        _urlBuilderMock.Setup(x => x.BuildUrl(nerEntities))
            .Returns(expectedUrl);
        
        // Setup repository to return a created alert
        var createdAlert = Alert.Create(userId, query, expectedUrl, DateTime.UtcNow);
        _alertRepositoryMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync(new List<Alert> { createdAlert });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("gaming laptop", result.Name);
        Assert.Equal(expectedUrl, result.Url);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Handle_ZeroKeywords_NerExtractsNothing_ReturnsError()
    {
        // Arrange
        var userId = 1L;
        var query = "xyz123 nonsense";
        var command = new CreateAlertFromNaturalLanguageCommand(userId, query);
        
        var nerEntities = new NerEntities(
            Keywords: new List<string>(),
            Brand: null,
            MaxPrice: null,
            MinPrice: null,
            Category: null,
            Location: null
        );
        
        _nerServiceMock.Setup(x => x.ExtractEntitiesAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nerEntities);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NerExtractionFailedException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Empty(exception.ExtractedEntities.Keywords);
    }

    [Fact]
    public async Task Handle_NerTimeout_PropagatesTimeoutException()
    {
        // Arrange
        var userId = 1L;
        var query = "iphone 13";
        var command = new CreateAlertFromNaturalLanguageCommand(userId, query);
        
        _nerServiceMock.Setup(x => x.ExtractEntitiesAsync(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("NER service timed out"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TimeoutException>(
            () => _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("NER service timed out", exception.Message);
    }
}