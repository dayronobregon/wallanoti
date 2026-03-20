using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Notifications.Application.SaveOnNewItemsFound;

public sealed class SaveNotificationOnNewItemsFound : IDomainEventHandler<NewItemsFoundEvent>
{
    private const string PriceDropSuffix = "(Baja de Precio)";
    private readonly INotificationRepository _notificationRepository;
    private readonly IEventBus _eventBus;
    private readonly TimeProvider _timeProvider;

    public SaveNotificationOnNewItemsFound(INotificationRepository notificationRepository, IEventBus eventBus, TimeProvider timeProvider)
    {
        _notificationRepository = notificationRepository;
        _eventBus = eventBus;
        _timeProvider = timeProvider;
    }

    public async Task Handle(NewItemsFoundEvent @event)
    {
        var items = @event.Items?.ToList() ?? [];
        var notifications = new List<Notification>();
        var events = new List<DomainEvent>();
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var snapshotsByUrl = await GetLatestSnapshotsByUrl(@event.UserId, items);
        
        foreach (var item in items)
        {
            var notification = Notification.Create(
                Guid.NewGuid(),
                @event.UserId,
                BuildNotificationTitle(item, snapshotsByUrl),
                item.Description,
                item.Price,
                item.Images,
                item.Location.FullLocation,
                Url.CreateFromSlug(item.WebSlug),
                now
            );

            notifications.Add(notification);
            events.AddRange(notification.PullDomainEvents());
        }

        await _notificationRepository.AddRangeAsync(notifications);

        await _eventBus.Publish(events);
    }

    private async Task<IReadOnlyDictionary<string, LastNotifiedItemSnapshot>> GetLatestSnapshotsByUrl(
        long userId,
        IReadOnlyCollection<Item> items)
    {
        if (items.Count == 0)
        {
            return new Dictionary<string, LastNotifiedItemSnapshot>();
        }

        var urls = items
            .Select(item => Url.CreateFromSlug(item.WebSlug).Value)
            .Distinct()
            .ToArray();

        return await _notificationRepository.GetLatestByUserAndUrls(userId, urls);
    }

    private static string BuildNotificationTitle(
        Item item,
        IReadOnlyDictionary<string, LastNotifiedItemSnapshot> snapshotsByUrl)
    {
        var url = Url.CreateFromSlug(item.WebSlug).Value;
        var isPriceDrop = item.Price is not null &&
                          snapshotsByUrl.TryGetValue(url, out var snapshot) &&
                          item.Price.CurrentPrice < snapshot.LastNotifiedCurrentPrice;

        return isPriceDrop ? $"{item.Title} {PriceDropSuffix}" : item.Title;
    }
}
