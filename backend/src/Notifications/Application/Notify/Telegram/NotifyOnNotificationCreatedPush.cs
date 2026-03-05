using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Notifications.Application.Notify.Telegram;

public sealed class NotifyOnNotificationCreatedPush : IDomainEventHandler<NotificationCreatedEvent>
{
    private readonly IPushNotificationSender _pushNotificationSender;

    public NotifyOnNotificationCreatedPush(IPushNotificationSender pushNotificationSender)
    {
        _pushNotificationSender = pushNotificationSender;
    }

    public async Task Handle(NotificationCreatedEvent @event)
    {
        var notificationEvent = @event.Notification;

        await _pushNotificationSender.Notify(notificationEvent);
    }
}