using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Wallanoti.Src.Alerts.Application.SearchNewItems;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Alerts._2_Application.SearchNewItems;

public class ItemSearcherTest
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new();
    private readonly Mock<IWallapopRepository> _wallapopRepositoryMock = new();
    private readonly Mock<IPushNotificationSender> _pushNotificationSenderMock = new();
    private readonly IDistributedCache _cache;
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly ItemSearcher _sut;

    public ItemSearcherTest()
    {
        _eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);
        _alertRepositoryMock.Setup(x => x.Update(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);
        _alertRepositoryMock.Setup(x => x.TouchAlert(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(1);
        _alertRepositoryMock.Setup(x => x.UpdateLastSearchedAt(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync(1);
        _pushNotificationSenderMock.Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Notifications:OwnerChatId"] = "999"
            })
            .Build();

        _sut = new ItemSearcher(
            _eventBusMock.Object,
            _alertRepositoryMock.Object,
            _wallapopRepositoryMock.Object,
            _pushNotificationSenderMock.Object,
            configuration,
            _cache,
            _timeProvider);
    }

    [Fact]
    public async Task Execute_EmitsOnlyFirstTimeAndPriceDropItems()
    {
        var now = DateTime.UtcNow;
        var alert = BuildAlert(now.AddMinutes(-30), now.AddMinutes(-20), now.AddMinutes(-20));

        var items = new List<Item>
        {
            BuildItem("item-first-time", "first-time", now, 120),
            BuildItem("item-price-drop", "price-drop", now.AddMinutes(1), 80),
            BuildItem("item-no-drop", "no-drop", now.AddMinutes(2), 100),
            BuildItem("item-higher-price", "higher-price", now.AddMinutes(3), 150),
            BuildItem("item-null-price", "null-price", now.AddMinutes(4), null),
            BuildItem("item-cached", "cached", now.AddMinutes(5), 99)
        };
        _cache.SetString(alert.GetCacheKey(), JsonSerializer.Serialize(new List<string> { "item-cached" }));
        _cache.SetString(GetPriceCacheKey(alert.GetCacheKey()), JsonSerializer.Serialize(new Dictionary<string, double>
        {
            ["item-price-drop"] = 100,
            ["item-no-drop"] = 100,
            ["item-higher-price"] = 140,
            ["item-null-price"] = 110,
            ["item-cached"] = 99
        }));

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events =>
            events.OfType<NewItemsFoundEvent>().Single().UserId == alert.UserId &&
            events.OfType<NewItemsFoundEvent>().Single().Items!.Select(i => i.Id).OrderBy(x => x)
                .SequenceEqual(new[] { "item-first-time", "item-price-drop" }.OrderBy(x => x)))), Times.Once);

        _alertRepositoryMock.Verify(x => x.Update(alert), Times.Once);
        Assert.NotNull(_cache.GetString(alert.GetCacheKey()));

        var priceCache = JsonSerializer.Deserialize<Dictionary<string, double>>(
            _cache.GetString(GetPriceCacheKey(alert.GetCacheKey()))!);
        Assert.NotNull(priceCache);
        Assert.Equal(80, priceCache!["item-price-drop"]);
        Assert.Equal(150, priceCache["item-higher-price"]);
        Assert.Equal(99, priceCache["item-cached"]);
    }

    [Fact]
    public async Task Execute_WhenNoNewItems_DoesNotPublish_ButUpdatesLastSearchedAtAndCache()
    {
        var now = DateTime.UtcNow;
        var alert = BuildAlert(now, now, now);
        DateTime? lastSearchedAt = null;
        var items = new List<Item>
        {
            BuildItem("item-1", "item-1", now.AddMinutes(-10), 10)
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);
        _alertRepositoryMock
            .Setup(x => x.UpdateLastSearchedAt(alert.Id, It.IsAny<DateTime>()))
            .Callback<Guid, DateTime>((_, updatedAt) => lastSearchedAt = updatedAt)
            .ReturnsAsync(1);

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Never);
        _alertRepositoryMock.Verify(x => x.UpdateLastSearchedAt(alert.Id, It.IsAny<DateTime>()), Times.Once);
        _alertRepositoryMock.Verify(x => x.Update(It.IsAny<Alert>()), Times.Never);
        Assert.NotNull(lastSearchedAt);
        Assert.Equal(lastSearchedAt, alert.LastSearchedAt);
        Assert.NotNull(_cache.GetString(alert.GetCacheKey()));
        var priceCache = JsonSerializer.Deserialize<Dictionary<string, double>>(
            _cache.GetString(GetPriceCacheKey(alert.GetCacheKey()))!);
        Assert.NotNull(priceCache);
        Assert.Equal(10, priceCache!["item-1"]);
    }

    [Fact]
    public async Task Execute_WhenItemsAreOnlyCached_DoesNotQueryHistoryAndDoesNotPublish()
    {
        var createdAt = DateTime.UtcNow;
        var alert = BuildAlert(createdAt.AddMinutes(-30), createdAt.AddMinutes(-20), createdAt.AddMinutes(-20));
        DateTime? lastSearchedAt = null;
        var cachedId = "item-cached";
        var items = new List<Item>
        {
            BuildItem(cachedId, "cached", createdAt.AddMinutes(10), 30)
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);
        _cache.SetString(alert.GetCacheKey(), JsonSerializer.Serialize(new List<string> { cachedId }));
        _alertRepositoryMock
            .Setup(x => x.UpdateLastSearchedAt(alert.Id, It.IsAny<DateTime>()))
            .Callback<Guid, DateTime>((_, updatedAt) => lastSearchedAt = updatedAt)
            .ReturnsAsync(1);

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Never);
        _alertRepositoryMock.Verify(x => x.UpdateLastSearchedAt(alert.Id, It.IsAny<DateTime>()), Times.Once);
        _alertRepositoryMock.Verify(x => x.Update(It.IsAny<Alert>()), Times.Never);
        Assert.NotNull(lastSearchedAt);
        Assert.Equal(lastSearchedAt, alert.LastSearchedAt);
        Assert.NotNull(_cache.GetString(alert.GetCacheKey()));
    }

    [Fact]
    public async Task Execute_WhenWallapopRepositoryThrows_NotifiesOwnerThroughPushSender()
    {
        var alert = BuildAlert(DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-20), DateTime.UtcNow.AddMinutes(-20));

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ThrowsAsync(new InvalidOperationException("boom"));

        await _sut.Execute();

        _pushNotificationSenderMock.Verify(x => x.Notify(
                999,
                It.Is<string>(message =>
                    message.Contains("alerts.wallapop.latest") &&
                    message.Contains("InvalidOperationException: boom") &&
                    message.Contains(alert.Id.ToString()))),
            Times.Once);
    }

    private static Alert BuildAlert(DateTime createdAt, DateTime? updatedAt = null, DateTime? lastSearchedAt = null, bool isActive = true)
    {
        return new Alert(Guid.NewGuid(), 1, "alert", "https://es.wallapop.com/item/slug", createdAt, updatedAt,
            isActive, lastSearchedAt);
    }

    private static Item BuildItem(string id, string slug, DateTime createdAt, double? currentPrice, DateTime? modifiedAt = null)
    {
        return new Item
        {
            Id = id,
            WallapopUserId = "user",
            Title = "title",
            Description = "desc",
            CategoryId = 1,
            Price = currentPrice.HasValue ? Price.Create(currentPrice.Value, null) : null,
            Images = new List<string>(),
            Location = Location.Create("city", "region"),
            Shipping = false,
            Favorited = false,
            Reserved = false,
            WebSlug = slug,
            CreatedAt = new DateTimeOffset(createdAt).ToUnixTimeMilliseconds(),
            ModifiedAt = new DateTimeOffset(modifiedAt ?? createdAt).ToUnixTimeMilliseconds()
        };
    }

    private static string GetPriceCacheKey(string alertCacheKey)
    {
        return $"{alertCacheKey}:prices";
    }
}
