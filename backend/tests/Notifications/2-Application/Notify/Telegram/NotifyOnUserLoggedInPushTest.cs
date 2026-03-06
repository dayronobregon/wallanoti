using Moq;
using Wallanoti.Src.Notifications.Application.Notify.Telegram;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Users.Domain.Events;
using Wallanoti.Src.Users.Domain.ValueObjects;

namespace Wallanoti.Tests.Notifications._2_Application.Notify.Telegram;

public class NotifyOnUserLoggedInPushTest
{
    private readonly Mock<IPushNotificationSender> _pushSenderMock = new();

    [Fact]
    public async Task Handle_ShouldSendVerificationCodeToUser()
    {
        var handler = new NotifyOnUserLoggedInPush(_pushSenderMock.Object);
        _pushSenderMock.Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var verificationCode = new VerificationCode("123456");
        var @event = new UserLoggedInDomainEvent(15, verificationCode);

        await handler.Handle(@event);

        _pushSenderMock.Verify(x => x.Notify(15, verificationCode.Value), Times.Once);
    }
}
