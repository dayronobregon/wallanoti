using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class SafeTelegramMessageResolverTest
{
    private const string StandardFriendlyErrorMessage = "Ha ocurrido un error. Será notificado al administrador.";

    private readonly Mock<IPushNotificationSender> _pushNotificationSenderMock = new();

    [Fact]
    public async Task Execute_WhenCoreFails_SendsFriendlyErrorMessage()
    {
        const long chatId = 100L;
        var message = new Message { Chat = new Chat { Id = chatId } };

        _pushNotificationSenderMock
            .Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = new TestSafeResolver(_pushNotificationSenderMock.Object) { ShouldThrowCore = true };

        await sut.Execute(message);

        _pushNotificationSenderMock.Verify(
            x => x.Notify(chatId, StandardFriendlyErrorMessage),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenCoreAndFriendlyMessageFail_DoesNotThrow()
    {
        var message = new Message { Chat = new Chat { Id = 200L } };

        _pushNotificationSenderMock
            .Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("telegram unavailable"));

        var sut = new TestSafeResolver(_pushNotificationSenderMock.Object) { ShouldThrowCore = true };

        var exception = await Record.ExceptionAsync(() => sut.Execute(message));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Execute_WhenCoreSucceeds_DoesNotSendFriendlyErrorMessage()
    {
        var message = new Message { Chat = new Chat { Id = 300L } };

        var sut = new TestSafeResolver(_pushNotificationSenderMock.Object);

        await sut.Execute(message);

        _pushNotificationSenderMock.Verify(
            x => x.Notify(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never);
    }

    private sealed class TestSafeResolver : SafeTelegramMessageResolver
    {
        public bool ShouldThrowCore { get; init; }

        public TestSafeResolver(IPushNotificationSender pushNotificationSender)
            : base(pushNotificationSender, NullLogger.Instance)
        {
        }

        protected override Task ExecuteCore(Message message)
        {
            if (ShouldThrowCore)
            {
                throw new InvalidOperationException("core failure");
            }

            return Task.CompletedTask;
        }
    }
}
