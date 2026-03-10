using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Tests.Alerts._1_Domain;

public class AlertLastSearchedAtTest
{
    [Fact]
    public void Create_ShouldInitializeLastSearchedAtAsNull()
    {
        // Arrange & Act
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");

        // Assert
        Assert.Null(alert.LastSearchedAt);
        Assert.NotNull(alert.CreatedAt);
        Assert.NotNull(alert.UpdatedAt);
    }

    [Fact]
    public void RecordSearch_ShouldUpdateLastSearchedAtAndUpdatedAt()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");
        var initialUpdatedAt = alert.UpdatedAt;
        
        Thread.Sleep(10); // Ensure time passes

        // Act
        alert.RecordSearch();

        // Assert
        Assert.NotNull(alert.LastSearchedAt);
        Assert.True(alert.LastSearchedAt > initialUpdatedAt);
    }

    [Fact]
    public void RecordSearch_CalledMultipleTimes_ShouldUpdateToLatestTimestamp()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");
        
        // Act
        alert.RecordSearch();
        var firstSearch = alert.LastSearchedAt;
        
        Thread.Sleep(10);
        
        alert.RecordSearch();
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
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");
        alert.RecordSearch();
        var lastSearchedAt = alert.LastSearchedAt;
        var initialUpdatedAt = alert.UpdatedAt;
        
        Thread.Sleep(10);

        // Act
        alert.Deactivate();

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
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");
        alert.RecordSearch();
        var lastSearchedAt = alert.LastSearchedAt;
        alert.Deactivate();
        
        Thread.Sleep(10);

        // Act
        alert.Activate();

        // Assert
        Assert.True(alert.IsActive);
        Assert.Equal(lastSearchedAt, alert.LastSearchedAt); // Should NOT change
        Assert.NotEqual(alert.UpdatedAt, alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithItems_ShouldRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");
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
        alert.NewSearch(items);

        // Assert
        Assert.NotNull(alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithNullItems_ShouldNotRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");

        // Act
        alert.NewSearch(null);

        // Assert
        Assert.Null(alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithEmptyList_ShouldNotRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search");

        // Act
        alert.NewSearch(new List<Item>());

        // Assert
        Assert.Null(alert.LastSearchedAt);
    }
}
