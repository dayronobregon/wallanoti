using Moq;
using Wallanoti.Src.Notifications.Application.Notify.Telegram;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Notifications._2_Application.Notify.Telegram;

public class NotifyOnNotificationCreatedPushTest
{
    private readonly Mock<IPushNotificationSender> _pushSenderMock = new();

    [Fact]
    public async Task Handle_ShouldSendPushNotification()
    {
        var handler = new NotifyOnNotificationCreatedPush(_pushSenderMock.Object);
        _pushSenderMock.Setup(x => x.Notify(It.IsAny<Notification>())).Returns(Task.CompletedTask);
        var notification = Notification.Create(Guid.NewGuid(), 3, "title", "description", Price.Create(10, 12),
            new List<string>(), "city", Url.CreateFromSlug("slug"));
        var @event = new NotificationCreatedEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString(), notification);

        await handler.Handle(@event);

        _pushSenderMock.Verify(x => x.Notify(notification), Times.Once);
    }
}
