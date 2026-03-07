using Moq;
using Wallanoti.Src.Notifications.Application.Notify.Web;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Notifications._2_Application.Notify.Web;

public class NotifyOnNotificationCreatedWebTest
{
    private readonly Mock<IWebNotificationSender> _webSenderMock = new();

    [Fact]
    public async Task Handle_ShouldSendWebNotification()
    {
        var handler = new NotifyOnNotificationCreatedWeb(_webSenderMock.Object);
        _webSenderMock.Setup(x => x.Notify(It.IsAny<Notification>())).Returns(Task.CompletedTask);
        var notification = Notification.Create(Guid.NewGuid(), 3, "title", "description", Price.Create(10, 12),
            new List<string>(), "city", Url.CreateFromSlug("slug"));
        var @event = new NotificationCreatedEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("o"),
            notification);

        await handler.Handle(@event);

        _webSenderMock.Verify(x => x.Notify(notification), Times.Once);
    }
}
