using Wallanoti.Src.Alerts.Domain.DTOs;

namespace Wallanoti.Tests.Alerts._1_Domain;

public class NerEntitiesTest
{
    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateImmutableRecord()
    {
        // Arrange
        var keywords = new List<string> { "iphone", "14", "pro" };
        var brand = "Apple";
        var maxPrice = 300m;
        var minPrice = 100m;
        var category = "electronics";
        var location = "Barcelona";

        // Act
        var nerEntities = new NerEntities(keywords, brand, maxPrice, minPrice, category, location);

        // Assert
        Assert.Equal(keywords.AsReadOnly(), nerEntities.Keywords);
        Assert.Equal(brand, nerEntities.Brand);
        Assert.Equal(maxPrice, nerEntities.MaxPrice);
        Assert.Equal(minPrice, nerEntities.MinPrice);
        Assert.Equal(category, nerEntities.Category);
        Assert.Equal(location, nerEntities.Location);
    }

    [Fact]
    public void Constructor_WithOnlyKeywords_ShouldCreateWithOptionalNulls()
    {
        // Arrange
        var keywords = new List<string> { "gaming", "laptop" };

        // Act
        var nerEntities = new NerEntities(keywords, null, null, null, null, null);

        // Assert
        Assert.Equal(keywords.AsReadOnly(), nerEntities.Keywords);
        Assert.Null(nerEntities.Brand);
        Assert.Null(nerEntities.MaxPrice);
        Assert.Null(nerEntities.MinPrice);
        Assert.Null(nerEntities.Category);
        Assert.Null(nerEntities.Location);
    }

    [Fact]
    public void Keywords_ShouldBeReadOnlyList()
    {
        // Arrange
        var keywords = new List<string> { "test", "keywords" };

        // Act
        var nerEntities = new NerEntities(keywords, null, null, null, null, null);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<string>>(nerEntities.Keywords);
    }

    [Fact]
    public void Record_ShouldBeSealed()
    {
        // Arrange & Act & Assert
        Assert.True(typeof(NerEntities).IsSealed);
    }

    [Fact]
    public void TwoInstancesWithSameKeywordReferences_ShouldBeEqual()
    {
        // Arrange - use same reference for Keywords to ensure equality
        var keywords = new List<string> { "iphone" }.AsReadOnly();

        var nerEntities1 = new NerEntities(keywords, "Apple", 300m, null, null, "Barcelona");
        var nerEntities2 = new NerEntities(keywords, "Apple", 300m, null, null, "Barcelona");

        // Act & Assert
        Assert.Equal(nerEntities1, nerEntities2);
    }

    [Fact]
    public void TwoInstancesWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var keywords1 = new List<string> { "iphone" }.AsReadOnly();
        var keywords2 = new List<string> { "samsung" }.AsReadOnly();

        var nerEntities1 = new NerEntities(keywords1, "Apple", 300m, null, null, "Barcelona");
        var nerEntities2 = new NerEntities(keywords2, "Samsung", 200m, null, null, "Madrid");

        // Act & Assert
        Assert.NotEqual(nerEntities1, nerEntities2);
    }
}
