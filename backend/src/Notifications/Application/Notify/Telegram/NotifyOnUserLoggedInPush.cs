using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Domain.Events;

namespace Wallanoti.Src.Notifications.Application.Notify.Telegram;

public sealed class NotifyOnUserLoggedInPush : IDomainEventHandler<UserLoggedInDomainEvent>
{
    private readonly IPushNotificationSender _pushNotificationSender;

    public NotifyOnUserLoggedInPush(IPushNotificationSender pushNotificationSender)
    {
        _pushNotificationSender = pushNotificationSender;
    }

    public async Task Handle(UserLoggedInDomainEvent @event)
    {
        await _pushNotificationSender.Notify(@event.TelegramUserId, @event.VerificationCode.Value);
    }
}