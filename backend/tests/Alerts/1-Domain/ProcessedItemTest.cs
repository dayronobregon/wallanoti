using Wallanoti.Src.Alerts.Domain.Entities;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.ValueObjects;
using Wallanoti.Src.Shared.Domain.Events;
using Xunit;

namespace Wallanoti.Tests.Alerts._1_Domain;

public class ProcessedItemTest
{
    private static readonly Guid SampleAlertId = Guid.NewGuid();
    private static readonly string SampleItemId = "sample-item-id";
    private static readonly long SampleModifiedAtMs = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
    private static readonly Price SamplePrice = Price.Create(100.0, null);

    [Fact]
    public void Ctor_WithValidArguments_SetsProperties()
    {
        // Arrange & Act
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);

        // Assert
        Assert.Equal(SampleAlertId, processedItem.AlertId);
        Assert.Equal(SampleItemId, processedItem.ItemId);
        Assert.Equal(SampleModifiedAtMs, processedItem.LastModifiedAtMs);
        Assert.Equal(SamplePrice.CurrentPrice, processedItem.LastPrice.CurrentPrice);
    }

    [Fact]
    public void Ctor_WithEmptyAlertId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ProcessedItem(Guid.Empty, SampleItemId, SampleModifiedAtMs, SamplePrice));
    }

    [Fact]
    public void Ctor_WithNullItemId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ProcessedItem(SampleAlertId, null!, SampleModifiedAtMs, SamplePrice));
    }

    [Fact]
    public void Ctor_WithEmptyItemId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new ProcessedItem(SampleAlertId, "", SampleModifiedAtMs, SamplePrice));
    }

    [Fact]
    public void Equals_SameAlertIdAndItemId_ReturnsTrue()
    {
        // Arrange
        var item1 = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item2 = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs + 3600000, Price.Create(90.0, null));

        // Act & Assert
        Assert.True(item1 == item2);
        Assert.Equal(item1, item2);
    }

    [Fact]
    public void Equals_DifferentAlertId_ReturnsFalse()
    {
        // Arrange
        var item1 = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item2 = new ProcessedItem(Guid.NewGuid(), SampleItemId, SampleModifiedAtMs, SamplePrice);

        // Act & Assert
        Assert.False(item1 == item2);
    }

    [Fact]
    public void Equals_DifferentItemId_ReturnsFalse()
    {
        // Arrange
        var item1 = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item2 = new ProcessedItem(SampleAlertId, "different-item-id", SampleModifiedAtMs, SamplePrice);

        // Act & Assert
        Assert.False(item1 == item2);
    }

    [Fact]
    public void GetHashCode_SameKey_ReturnsSameHash()
    {
        // Arrange
        var item1 = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item2 = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs + 3600000, Price.Create(90.0, null));

        // Act & Assert
        Assert.Equal(item1.GetHashCode(), item2.GetHashCode());
    }

    [Fact]
    public void DetectChange_NewerModifiedAt_ReturnsUpdate()
    {
        // Arrange
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var newerMs = SampleModifiedAtMs + 3600000;
        var item = CreateItem(SampleItemId, newerMs, 100.0);

        // Act
        var changeType = processedItem.DetectChange(item);

        // Assert
        Assert.Equal(ChangeType.Update, changeType);
    }

    [Fact]
    public void DetectChange_PriceDrop_ReturnsPriceDrop()
    {
        // Arrange
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item = CreateItem(SampleItemId, SampleModifiedAtMs, 90.0);

        // Act
        var changeType = processedItem.DetectChange(item);

        // Assert
        Assert.Equal(ChangeType.PriceDrop, changeType);
    }

    [Fact]
    public void DetectChange_PriceDropAndUpdate_ReturnsPriceDrop()
    {
        // Arrange
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var newerMs = SampleModifiedAtMs + 3600000;
        var item = CreateItem(SampleItemId, newerMs, 90.0);

        // Act
        var changeType = processedItem.DetectChange(item);

        // Assert
        Assert.Equal(ChangeType.PriceDrop, changeType);
    }

    [Fact]
    public void DetectChange_NoChange_ReturnsNone()
    {
        // Arrange
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item = CreateItem(SampleItemId, SampleModifiedAtMs, 100.0);

        // Act
        var changeType = processedItem.DetectChange(item);

        // Assert
        Assert.Equal(ChangeType.None, changeType);
    }

    [Fact]
    public void UpdateFrom_UpdatesFields()
    {
        // Arrange
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var newMs = SampleModifiedAtMs + 3600000;
        var newPrice = Price.Create(90.0, null);
        var item = CreateItem(SampleItemId, newMs, 90.0);

        // Act
        processedItem.UpdateFrom(item);

        // Assert
        Assert.Equal(newMs, processedItem.LastModifiedAtMs);
        Assert.Equal(90.0, processedItem.LastPrice.CurrentPrice);
    }

    [Fact]
    public void UpdateFrom_WithChange_RecordsItemChangesFoundEvent()
    {
        // Arrange
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item = CreateItem(SampleItemId, SampleModifiedAtMs, 90.0);

        // Act
        processedItem.UpdateFrom(item);

        // Assert
        var events = processedItem.PullDomainEvents();
        Assert.Single(events);
        var ev = Assert.IsType<ItemChangesFoundEvent>(events[0]);
        Assert.Equal(ChangeType.PriceDrop, ev.ChangeType);
        Assert.Equal(SampleAlertId, ev.AlertId);
        Assert.Equal(SampleItemId, ev.ItemId);
    }

    [Fact]
    public void UpdateFrom_NoChange_NoEventRecorded()
    {
        // Arrange
        var processedItem = new ProcessedItem(SampleAlertId, SampleItemId, SampleModifiedAtMs, SamplePrice);
        var item = CreateItem(SampleItemId, SampleModifiedAtMs, 100.0);

        // Act
        processedItem.UpdateFrom(item);

        // Assert
        var events = processedItem.PullDomainEvents();
        Assert.Empty(events);
    }

    private static Item CreateItem(string id, long modifiedAtMs, double price)
    {
        return new()
        {
            Id = id,
            WallapopUserId = "test-user",
            Title = "Test Item",
            Description = "Test Description",
            CategoryId = 1,
            Price = Price.Create(price, null),
            Images = new List<string>(),
            Reserved = false,
            Location = Location.Create("Madrid", "Spain"),
            Shipping = false,
            Favorited = false,
            WebSlug = "test-slug",
            CreatedAt = modifiedAtMs - 3600000,
            ModifiedAt = modifiedAtMs
        };
    }
}