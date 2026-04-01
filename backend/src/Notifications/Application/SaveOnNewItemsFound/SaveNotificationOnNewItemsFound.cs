using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Notifications.Application.SaveOnNewItemsFound;

public sealed class SaveNotificationOnNewItemsFound : IDomainEventHandler<NewItemsFoundEvent>
{
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
        var notifications = new List<Notification>();
        var events = new List<DomainEvent>();
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var labeled in @event.LabeledItems ?? [])
        {
            var item = labeled.Item;
            var title = FormatTitle(item.Title, labeled.Label);

            var notification = Notification.Create(
                Guid.NewGuid(),
                @event.UserId,
                title,
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

    private static string FormatTitle(string title, ItemNotificationLabel label) => label switch
    {
        ItemNotificationLabel.PriceDrop => $"{title} (Baja de Precio)",
        _ => title
    };
}
