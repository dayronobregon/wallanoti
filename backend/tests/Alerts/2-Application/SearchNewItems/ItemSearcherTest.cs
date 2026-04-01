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
    public async Task Execute_PublishesNewItemsAndUpdatesCache()
    {
        var alert = BuildAlert(DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-20), DateTime.UtcNow.AddMinutes(-20));
        var newerTime = DateTime.UtcNow;
        var items = new List<Item>
        {
            BuildItem("item-1", newerTime),
            BuildItem("item-2", newerTime.AddMinutes(1))
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events =>
            events.OfType<NewItemsFoundEvent>().Single().Items!.Count() == items.Count &&
            events.OfType<NewItemsFoundEvent>().Single().UserId == alert.UserId)), Times.Once);

        _alertRepositoryMock.Verify(x => x.Update(alert), Times.Once);
        Assert.NotNull(_cache.GetString(alert.GetCacheKey()));
    }

    [Fact]
    public async Task Execute_WhenNoNewItems_DoesNotPublishOrCache_ButUpdatesLastSearchedAt()
    {
        var now = DateTime.UtcNow;
        var alert = BuildAlert(now, now, now);
        DateTime? lastSearchedAt = null;
        var items = new List<Item>
        {
            BuildItem("item-1", now.AddMinutes(-10))
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
        Assert.Null(_cache.GetString(alert.GetCacheKey()));
    }

    [Fact]
    public async Task Execute_SkipsItemsAlreadyInCacheWithSamePrice()
    {
        var createdAt = DateTime.UtcNow;
        var alert = BuildAlert(createdAt.AddMinutes(-30), createdAt.AddMinutes(-20), createdAt.AddMinutes(-20));
        DateTime? lastSearchedAt = null;
        const string cachedId = "item-cached";
        const double cachedPrice = 10.0;
        var items = new List<Item>
        {
            BuildItem(cachedId, createdAt.AddMinutes(10), price: cachedPrice)
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);
        SetCacheWithPrices(alert.GetCacheKey(), new Dictionary<string, double> { [cachedId] = cachedPrice });
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
    public async Task Execute_WhenItemHasPriceDrop_PublishesEventWithPriceDropLabel()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-30);
        var alert = BuildAlert(createdAt, createdAt.AddMinutes(10), createdAt.AddMinutes(10));
        const string itemId = "item-drop";
        const double oldPrice = 50.0;
        const double newPrice = 35.0;
        var itemTime = DateTime.UtcNow;
        var items = new List<Item>
        {
            BuildItem(itemId, itemTime, price: newPrice)
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);
        SetCacheWithPrices(alert.GetCacheKey(), new Dictionary<string, double> { [itemId] = oldPrice });

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events =>
            events.OfType<NewItemsFoundEvent>().Single().LabeledItems!
                .Any(li => li.Item.Id == itemId && li.Label == ItemNotificationLabel.PriceDrop))),
            Times.Once);

        _alertRepositoryMock.Verify(x => x.Update(alert), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenItemHasPriceDrop_UpdatesCacheWithNewPrice()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-30);
        var alert = BuildAlert(createdAt, createdAt.AddMinutes(10), createdAt.AddMinutes(10));
        const string itemId = "item-drop";
        const double oldPrice = 50.0;
        const double newPrice = 35.0;
        var itemTime = DateTime.UtcNow;
        var items = new List<Item>
        {
            BuildItem(itemId, itemTime, price: newPrice)
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);
        SetCacheWithPrices(alert.GetCacheKey(), new Dictionary<string, double> { [itemId] = oldPrice });

        await _sut.Execute();

        var cachedRaw = _cache.GetString(alert.GetCacheKey());
        Assert.NotNull(cachedRaw);
        var cached = JsonSerializer.Deserialize<Dictionary<string, double>>(cachedRaw!);
        Assert.NotNull(cached);
        Assert.Equal(newPrice, cached![itemId]);
    }

    [Fact]
    public async Task Execute_WhenItemHasPriceDrop_ItemCarriesPreviousPriceInEvent()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-30);
        var alert = BuildAlert(createdAt, createdAt.AddMinutes(10), createdAt.AddMinutes(10));
        const string itemId = "item-drop";
        const double oldPrice = 50.0;
        const double newPrice = 35.0;
        var itemTime = DateTime.UtcNow;
        var items = new List<Item>
        {
            BuildItem(itemId, itemTime, price: newPrice)
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);
        SetCacheWithPrices(alert.GetCacheKey(), new Dictionary<string, double> { [itemId] = oldPrice });

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events =>
            events.OfType<NewItemsFoundEvent>().Single().LabeledItems!
                .Any(li =>
                    li.Item.Id == itemId &&
                    li.Item.Price!.PreviousPrice == oldPrice &&
                    li.Item.Price.CurrentPrice == newPrice))),
            Times.Once);
    }

    [Fact]
    public async Task Execute_NewItemsLabeledAsNew_InLabeledItems()
    {
        var alert = BuildAlert(DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-20), DateTime.UtcNow.AddMinutes(-20));
        var newerTime = DateTime.UtcNow;
        var items = new List<Item>
        {
            BuildItem("new-item-1", newerTime),
            BuildItem("new-item-2", newerTime.AddMinutes(1))
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events =>
            events.OfType<NewItemsFoundEvent>().Single().LabeledItems!
                .All(li => li.Label == ItemNotificationLabel.New))),
            Times.Once);
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

    private void SetCacheWithPrices(string cacheKey, Dictionary<string, double> prices)
    {
        _cache.SetString(cacheKey, JsonSerializer.Serialize(prices));
    }

    private static Alert BuildAlert(DateTime createdAt, DateTime? updatedAt = null, DateTime? lastSearchedAt = null, bool isActive = true)
    {
        return new Alert(Guid.NewGuid(), 1, "alert", "https://es.wallapop.com/item/slug", createdAt, updatedAt,
            isActive, lastSearchedAt);
    }

    private static Item BuildItem(string id, DateTime createdAt, DateTime? modifiedAt = null, double price = 10.0)
    {
        return new Item
        {
            Id = id,
            WallapopUserId = "user",
            Title = "title",
            Description = "desc",
            CategoryId = 1,
            Price = Price.Create(price, null),
            Images = new List<string>(),
            Location = Location.Create("city", "region"),
            Shipping = false,
            Favorited = false,
            Reserved = false,
            WebSlug = $"slug-{id}",
            CreatedAt = new DateTimeOffset(createdAt).ToUnixTimeMilliseconds(),
            ModifiedAt = new DateTimeOffset(modifiedAt ?? createdAt).ToUnixTimeMilliseconds()
        };
    }
}
