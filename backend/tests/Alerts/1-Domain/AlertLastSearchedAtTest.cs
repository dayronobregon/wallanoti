using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Tests.Alerts._1_Domain;

public class AlertLastSearchedAtTest
{
    private readonly DateTime _createdAt = new(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ShouldInitializeLastSearchedAtAsNull()
    {
        // Arrange & Act
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);

        // Assert
        Assert.Null(alert.LastSearchedAt);
        Assert.NotNull(alert.CreatedAt);
        Assert.NotNull(alert.UpdatedAt);
    }

    [Fact]
    public void RecordSearch_ShouldUpdateLastSearchedAtAndUpdatedAt()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);
        var searchTime = _createdAt.AddMinutes(5);

        // Act
        alert.RecordSearch(searchTime);

        // Assert
        Assert.NotNull(alert.LastSearchedAt);
        Assert.Equal(searchTime, alert.LastSearchedAt);
    }

    [Fact]
    public void RecordSearch_CalledMultipleTimes_ShouldUpdateToLatestTimestamp()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);
        var firstSearch = _createdAt.AddMinutes(5);
        var secondSearch = _createdAt.AddMinutes(10);

        // Act
        alert.RecordSearch(firstSearch);
        var firstResult = alert.LastSearchedAt;

        alert.RecordSearch(secondSearch);
        var secondResult = alert.LastSearchedAt;

        // Assert
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.True(secondResult > firstResult);
    }

    [Fact]
    public void Deactivate_ShouldUpdateUpdatedAt_ButNotLastSearchedAt()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);
        var searchTime = _createdAt.AddMinutes(5);
        alert.RecordSearch(searchTime);
        var lastSearchedAt = alert.LastSearchedAt;
        var initialUpdatedAt = alert.UpdatedAt;
        var deactivationTime = _createdAt.AddMinutes(10);

        // Act
        alert.Deactivate(deactivationTime);

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
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);
        var searchTime = _createdAt.AddMinutes(5);
        alert.RecordSearch(searchTime);
        var lastSearchedAt = alert.LastSearchedAt;
        alert.Deactivate(_createdAt.AddMinutes(10));
        var activationTime = _createdAt.AddMinutes(15);

        // Act
        alert.Activate(activationTime);

        // Assert
        Assert.True(alert.IsActive);
        Assert.Equal(lastSearchedAt, alert.LastSearchedAt); // Should NOT change
        Assert.NotEqual(alert.UpdatedAt, alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithItems_ShouldRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);
        var eventTime = _createdAt.AddMinutes(5);
        var searchTime = _createdAt.AddMinutes(5);
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
        alert.NewSearch(items, eventTime, searchTime);

        // Assert
        Assert.NotNull(alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithNullItems_ShouldNotRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);
        var eventTime = _createdAt.AddMinutes(5);
        var searchTime = _createdAt.AddMinutes(5);

        // Act
        alert.NewSearch(null, eventTime, searchTime);

        // Assert
        Assert.Null(alert.LastSearchedAt);
    }

    [Fact]
    public void NewSearch_WithEmptyList_ShouldNotRecordSearch()
    {
        // Arrange
        var alert = Alert.Create(userId: 123, name: "Test Alert", url: "https://es.wallapop.com/search", _createdAt, _createdAt);
        var eventTime = _createdAt.AddMinutes(5);
        var searchTime = _createdAt.AddMinutes(5);

        // Act
        alert.NewSearch(new List<Item>(), eventTime, searchTime);

        // Assert
        Assert.Null(alert.LastSearchedAt);
    }
}
