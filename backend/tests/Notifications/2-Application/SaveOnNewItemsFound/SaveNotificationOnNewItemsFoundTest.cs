using Moq;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Application.SaveOnNewItemsFound;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Notifications._2_Application.SaveOnNewItemsFound;

public class SaveNotificationOnNewItemsFoundTest
{
    private readonly Mock<INotificationRepository> _repositoryMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly SaveNotificationOnNewItemsFound _sut;

    public SaveNotificationOnNewItemsFoundTest()
    {
        _repositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()))
            .Returns(Task.CompletedTask);
        _repositoryMock
            .Setup(x => x.GetLatestByUserAndUrls(It.IsAny<long>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, LastNotifiedItemSnapshot>());
        _eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);
        _sut = new SaveNotificationOnNewItemsFound(_repositoryMock.Object, _eventBusMock.Object, _timeProvider);
    }

    [Fact]
    public async Task Handle_WithItems_ShouldPersistNotificationsAndPublishEvents()
    {
        var items = new List<Item> { BuildItem("one"), BuildItem("two") };
        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("o"), Guid.NewGuid(), 7,
            items);

        await _sut.Handle(@event);

        _repositoryMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(n => n.Count() == items.Count)),
            Times.Once);
        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events =>
            events.Count == items.Count && events.All(e => e is NotificationCreatedEvent))), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutItems_DoesNothing()
    {
        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("o"), Guid.NewGuid(), 7,
            null);

        await _sut.Handle(@event);

        _repositoryMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(n => !n.Any())), Times.Once);
        _eventBusMock.Verify(x => x.Publish(It.Is<List<DomainEvent>>(events => events.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPriceDropItem_ShouldPersistNotificationWithPriceDropTitleAndUpdatedPrice()
    {
        var item = BuildItem("price-drop", currentPrice: 15, previousPrice: 20);
        var itemUrl = Url.CreateFromSlug(item.WebSlug).Value;

        _repositoryMock
            .Setup(x => x.GetLatestByUserAndUrls(7, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, LastNotifiedItemSnapshot>
            {
                [itemUrl] = new(itemUrl, 20, DateTime.UtcNow.AddMinutes(-10))
            });

        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("o"), Guid.NewGuid(), 7,
            [item]);

        await _sut.Handle(@event);

        _repositoryMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Notification>>(notifications =>
            notifications.Count() == 1 &&
            notifications.First().Title == "item-price-drop (Baja de Precio)" &&
            notifications.First().Price != null &&
            notifications.First().Price!.CurrentPrice == 15
        )), Times.Once);
    }

    private static Item BuildItem(string slug, double currentPrice = 20, double? previousPrice = 25)
    {
        return new Item
        {
            Id = Guid.NewGuid().ToString(),
            WallapopUserId = "user",
            Title = $"item-{slug}",
            Description = "desc",
            CategoryId = 1,
            Price = Price.Create(currentPrice, previousPrice),
            Images = new List<string>(),
            Location = Location.Create("City", "Region"),
            Shipping = false,
            Favorited = false,
            Reserved = false,
            WebSlug = slug,
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}
