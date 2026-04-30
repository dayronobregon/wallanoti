using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Wallanoti.Src.Alerts.Domain.DTOs;
using Wallanoti.Src.Alerts.Infrastructure.Services;

namespace Wallanoti.Tests.Alerts._3_Infrastructure.Services;

public class HuggingFaceNerServiceTest
{
    private readonly Mock<ILogger<HuggingFaceNerService>> _loggerMock;
    private readonly NerServiceOptions _options;

    public HuggingFaceNerServiceTest()
    {
        _loggerMock = new Mock<ILogger<HuggingFaceNerService>>();
        _options = new NerServiceOptions
        {
            Provider = "HuggingFace",
            TimeoutSeconds = 30,
            ModelUrl = "https://api-inference.huggingface.co/models/dslim/bert-base-NER",
            ApiKey = "test-api-key"
        };
    }

    [Fact]
    public async Task ExtractEntitiesAsync_ValidResponse_ReturnsNerEntitiesWithKeywords()
    {
        // Arrange
        var hfResponse = JsonSerializer.Serialize(new[]
        {
            new { word = "iphone", start = 0, end = 6, entity = "B-PRODUCT" },
            new { word = "14", start = 7, end = 9, entity = "I-PRODUCT" },
            new { word = "pro", start = 10, end = 13, entity = "I-PRODUCT" }
        });

        var handlerMock = SetupHttpMessageHandlerMock(HttpStatusCode.OK, hfResponse);
        var httpClient = new HttpClient(handlerMock.Object);

        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await sut.ExtractEntitiesAsync("iphone 14 pro");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Iphone", result.Keywords);
        Assert.Contains("14", result.Keywords);
        Assert.Contains("Pro", result.Keywords);
    }

    [Fact]
    public async Task ExtractEntitiesAsync_ResponseWithBrand_ExtractsBrand()
    {
        // Arrange
        var hfResponse = JsonSerializer.Serialize(new[]
        {
            new { word = "apple", start = 0, end = 5, entity = "B-BRAND" }
        });

        var handlerMock = SetupHttpMessageHandlerMock(HttpStatusCode.OK, hfResponse);
        var httpClient = new HttpClient(handlerMock.Object);

        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await sut.ExtractEntitiesAsync("apple iphone");

        // Assert
        Assert.Equal("Apple", result.Brand);
    }

    [Fact]
    public async Task ExtractEntitiesAsync_ResponseWithPrice_ExtractsMaxPrice()
    {
        // Arrange
        var hfResponse = JsonSerializer.Serialize(new[]
        {
            new { word = "300", start = 0, end = 3, entity = "B-PRICE" }
        });

        var handlerMock = SetupHttpMessageHandlerMock(HttpStatusCode.OK, hfResponse);
        var httpClient = new HttpClient(handlerMock.Object);

        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await sut.ExtractEntitiesAsync("budget 300 euros");

        // Assert
        Assert.Equal(300m, result.MaxPrice);
    }

    [Fact]
    public async Task ExtractEntitiesAsync_ResponseWithLocation_ExtractsLocation()
    {
        // Arrange
        var hfResponse = JsonSerializer.Serialize(new[]
        {
            new { word = "madrid", start = 0, end = 6, entity = "B-LOCATION" }
        });

        var handlerMock = SetupHttpMessageHandlerMock(HttpStatusCode.OK, hfResponse);
        var httpClient = new HttpClient(handlerMock.Object);

        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await sut.ExtractEntitiesAsync("iphone madrid");

        // Assert
        Assert.Equal("Madrid", result.Location);
    }

    [Fact]
    public async Task ExtractEntitiesAsync_EmptyResponse_ReturnsEmptyKeywords()
    {
        // Arrange
        var hfResponse = JsonSerializer.Serialize(Array.Empty<object>());
        var handlerMock = SetupHttpMessageHandlerMock(HttpStatusCode.OK, hfResponse);
        var httpClient = new HttpClient(handlerMock.Object);

        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await sut.ExtractEntitiesAsync("random text without entities");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Keywords);
    }

    [Fact]
    public async Task ExtractEntitiesAsync_HttpTimeout_ThrowsTimeoutException()
    {
        // Issue 2 fix verification: TaskCanceledException should be converted to TimeoutException
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var httpClient = new HttpClient(handlerMock.Object);
        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(
            () => sut.ExtractEntitiesAsync("test input"));
    }

    [Fact]
    public async Task ExtractEntitiesAsync_MalformedJson_ReturnsEmptyEntities()
    {
        // Arrange
        var handlerMock = SetupHttpMessageHandlerMock(HttpStatusCode.OK, "not valid json {{{");
        var httpClient = new HttpClient(handlerMock.Object);

        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act
        var result = await sut.ExtractEntitiesAsync("test input");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Keywords);
    }

    [Fact]
    public async Task ExtractEntitiesAsync_HttpError_ThrowsHttpRequestException()
    {
        // Arrange
        var handlerMock = SetupHttpMessageHandlerMock(HttpStatusCode.InternalServerError, "Server error");
        var httpClient = new HttpClient(handlerMock.Object);

        var sut = new HuggingFaceNerService(httpClient, _options, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => sut.ExtractEntitiesAsync("test input"));
    }

    private Mock<HttpMessageHandler> SetupHttpMessageHandlerMock(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });

        return handlerMock;
    }
}
