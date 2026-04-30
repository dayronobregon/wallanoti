using Wallanoti.Src.Alerts.Domain.DTOs;
using Wallanoti.Src.Alerts.Domain.Services;
using Wallanoti.Src.Alerts.Infrastructure.Services;

namespace Wallanoti.Tests.Alerts._3_Infrastructure.Services;

public class WallapopUrlBuilderTest
{
    [Fact]
    public void BuildUrl_WithKeywordsOnly_ReturnsBaseUrlWithKeywords()
    {
        // Arrange
        var entities = new NerEntities(
            new List<string> { "gaming", "laptop" },
            null, null, null, null, null
        );
        var sut = new WallapopUrlBuilder();

        // Act
        var result = sut.BuildUrl(entities);

        // Assert
        Assert.StartsWith("https://es.wallapop.com/items?keywords=", result);
        Assert.Contains("gaming+laptop", result);
    }

    [Fact]
    public void BuildUrl_WithKeywordsAndMaxPrice_ReturnsUrlWithMaxPrice()
    {
        // Arrange
        var entities = new NerEntities(
            new List<string> { "iphone" },
            null, 300m, null, null, null
        );
        var sut = new WallapopUrlBuilder();

        // Act
        var result = sut.BuildUrl(entities);

        // Assert
        Assert.Contains("keywords=iphone", result);
        Assert.Contains("max_price=300", result);
    }

    [Fact]
    public void BuildUrl_WithKeywordsAndLocation_ReturnsUrlWithLocation()
    {
        // Arrange
        var entities = new NerEntities(
            new List<string> { "iphone" },
            null, null, null, null, "Barcelona"
        );
        var sut = new WallapopUrlBuilder();

        // Act
        var result = sut.BuildUrl(entities);

        // Assert
        Assert.Contains("keywords=iphone", result);
        Assert.Contains("location=Barcelona", result);
    }

    [Fact]
    public void BuildUrl_WithAllParameters_ReturnsUrlWithAllParams()
    {
        // Arrange
        var entities = new NerEntities(
            new List<string> { "iphone", "14", "pro" },
            "Apple",
            500m,
            100m,
            "electronics",
            "Madrid"
        );
        var sut = new WallapopUrlBuilder();

        // Act
        var result = sut.BuildUrl(entities);

        // Assert
        Assert.StartsWith("https://es.wallapop.com/items?", result);
        Assert.Contains("keywords=iphone+14+pro", result);
        Assert.Contains("max_price=500", result);
        Assert.Contains("min_price=100", result);
        Assert.Contains("category_id=", result);
        Assert.Contains("location=Madrid", result);
    }

    [Fact]
    public void BuildUrl_WithDuplicateKeywords_DeduplicatesKeywords()
    {
        // Arrange
        var entities = new NerEntities(
            new List<string> { "iphone", "iphone", "14", "14", "14" },
            null, null, null, null, null
        );
        var sut = new WallapopUrlBuilder();

        // Act
        var result = sut.BuildUrl(entities);

        // Assert
        Assert.Contains("keywords=iphone+14", result);
        Assert.DoesNotContain("iphone+iphone", result);
        Assert.DoesNotContain("14+14", result);
    }

    [Fact]
    public void BuildUrl_WithSpecialCharactersInKeywords_UrlEncodesSpecialChars()
    {
        // Arrange
        var entities = new NerEntities(
            new List<string> { "iPhone", "Pro Max", "128GB" },
            null, null, null, null, null
        );
        var sut = new WallapopUrlBuilder();

        // Act
        var result = sut.BuildUrl(entities);

        // Assert
        Assert.Contains("iPhone", result);
        Assert.Contains("Pro+Max", result);
        Assert.Contains("128GB", result);
    }
}
