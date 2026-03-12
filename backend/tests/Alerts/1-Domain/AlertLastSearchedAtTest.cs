using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Tests.Alerts._1_Domain;

public class AlertLastSearchedAtTest
{
    private readonly TimeProvider _timeProvider = TimeProvider.System;

    [Fact]
    public void Create_ShouldInitializeLastSearchedAtAsNull()
    {
        // Arrange & Act
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);

        // Assert
        Assert.Null(alert.LastSearchedAt);
        Assert.NotNull(alert.CreatedAt);
        Assert.NotNull(alert.UpdatedAt);
    }

    [Fact]
    public void RecordSearch_ShouldUpdateLastSearchedAtAndUpdatedAt()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);
        var initialUpdatedAt = alert.UpdatedAt;
        
        Thread.Sleep(10); // Ensure time passes

        // Act
        alert.RecordSearch(_timeProvider);

        // Assert
        Assert.NotNull(alert.LastSearchedAt);
        Assert.True(alert.LastSearchedAt > initialUpdatedAt);
    }

    [Fact]
    public void RecordSearch_CalledMultipleTimes_ShouldUpdateToLatestTimestamp()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);
        
        // Act
        alert.RecordSearch(_timeProvider);
        var firstSearch = alert.LastSearchedAt;
        
        Thread.Sleep(10);
        
        alert.RecordSearch(_timeProvider);
        var secondSearch = alert.LastSearchedAt;

        // Assert
        Assert.NotNull(firstSearch);
        Assert.NotNull(secondSearch);
        Assert.True(secondSearch > firstSearch);
    }

    [Fact]
    public void Deactivate_ShouldUpdateUpdatedAt_ButNotLastSearchedAt()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);
        alert.RecordSearch(_timeProvider);
        var lastSearchedAt = alert.LastSearchedAt;
        var initialUpdatedAt = alert.UpdatedAt;
        
        Thread.Sleep(10);

        // Act
        alert.Deactivate(_timeProvider);

        // Assert
        Assert.False(alert.IsActive);
        Assert.Equal(lastSearchedAt, alert.LastSearchedAt); // Should NOT change
        Assert.True(alert.UpdatedAt > initialUpdatedAt); // Should change
        Assert.NotEqual(alert.UpdatedAt, alert.LastSearchedAt);
    }

    [Fact]
    public void Activate_ShouldUpdateUpdatedAt_ButNotLastSearchedAt()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);
        alert.RecordSearch(_timeProvider);
        var lastSearchedAt = alert.LastSearchedAt;
        alert.Deactivate(_timeProvider);
        
        Thread.Sleep(10);

        // Act
        alert.Activate(_timeProvider);

        // Assert
        Assert.True(alert.IsActive);
        Assert.Equal(lastSearchedAt, alert.LastSearchedAt); // Should NOT change
        Assert.NotEqual(alert.UpdatedAt, alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithItems_ShouldRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);
        var items = new List<Item>
        {
            new()
            {
                Id = "item-1",
                WallapopUserId = "user",
                Title = "title",
                WebSlug = "slug",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };

        // Act
        alert.NewSearch(items, _timeProvider);

        // Assert
        Assert.NotNull(alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithNullItems_ShouldNotRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);

        // Act
        alert.NewSearch(null, _timeProvider);

        // Assert
        Assert.Null(alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithEmptyList_ShouldNotRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _timeProvider);

        // Act
        alert.NewSearch(new List<Item>(), _timeProvider);

        // Assert
        Assert.Null(alert.LastSearchedAt);
    }
}
