using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Notifications.Application.Notify.Web;

public sealed class NotifyOnNotificationCreatedWeb : IDomainEventHandler<NotificationCreatedEvent>
{
    private readonly IWebNotificationSender _notificationSender;

    public NotifyOnNotificationCreatedWeb(IWebNotificationSender notificationSender)
    {
        _notificationSender = notificationSender;
    }

    public async Task Handle(NotificationCreatedEvent @event)
    {
        var notificationEvent = @event.Notification;

        await _notificationSender.Notify(notificationEvent);
    }
}