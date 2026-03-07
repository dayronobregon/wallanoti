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
    private readonly ItemSearcher _sut;

    public ItemSearcherTest()
    {
        _eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);
        _alertRepositoryMock.Setup(x => x.Update(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);
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
            _cache);
    }

    [Fact]
    public async Task Execute_PublishesNewItemsAndUpdatesCache()
    {
        var alert = BuildAlert(DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-20));
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
    public async Task Execute_WhenNoNewItems_DoesNotPublishOrCache_ButTouchesAlert()
    {
        var now = DateTime.UtcNow;
        var alert = BuildAlert(now, now);
        var items = new List<Item>
        {
            BuildItem("item-1", now.AddMinutes(-10))
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Never);
        _alertRepositoryMock.Verify(x => x.Update(It.IsAny<Alert>()), Times.Once);
        Assert.Null(_cache.GetString(alert.GetCacheKey()));
    }

    [Fact]
    public async Task Execute_SkipsItemsAlreadyInCache()
    {
        var createdAt = DateTime.UtcNow;
        var alert = BuildAlert(createdAt.AddMinutes(-30), createdAt.AddMinutes(-20));
        var cachedId = "item-cached";
        var items = new List<Item>
        {
            BuildItem(cachedId, createdAt.AddMinutes(10))
        };

        _alertRepositoryMock.Setup(x => x.All()).ReturnsAsync(new[] { alert });
        _wallapopRepositoryMock.Setup(x => x.Latest(alert.Url)).ReturnsAsync(items);
        _cache.SetString(alert.GetCacheKey(), JsonSerializer.Serialize(new List<string> { cachedId }));

        await _sut.Execute();

        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Never);
        _alertRepositoryMock.Verify(x => x.Update(It.IsAny<Alert>()), Times.Once);
        Assert.NotNull(_cache.GetString(alert.GetCacheKey()));
    }

    [Fact]
    public async Task Execute_WhenWallapopRepositoryThrows_NotifiesOwnerThroughPushSender()
    {
        var alert = BuildAlert(DateTime.UtcNow.AddMinutes(-30), DateTime.UtcNow.AddMinutes(-20));

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

    private static Alert BuildAlert(DateTime createdAt, DateTime? updatedAt = null, bool isActive = true)
    {
        return new Alert(Guid.NewGuid(), 1, "alert", "https://es.wallapop.com/item/slug", createdAt, updatedAt,
            isActive);
    }

    private static Item BuildItem(string id, DateTime createdAt, DateTime? modifiedAt = null)
    {
        return new Item
        {
            Id = id,
            WallapopUserId = "user",
            Title = "title",
            Description = "desc",
            CategoryId = 1,
            Price = Price.Create(10, null),
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
