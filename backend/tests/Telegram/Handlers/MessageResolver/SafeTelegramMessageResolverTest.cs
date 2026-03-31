using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class SafeTelegramMessageResolverTest
{
    private const string StandardFriendlyErrorMessage = "Ha ocurrido un error. Será notificado al administrador.";

    private readonly Mock<ITelegramBotClient> _botClientMock = new();
    private readonly Mock<ITelegramBotConnection> _botConnectionMock = new();

    [Fact]
    public async Task Execute_WhenCoreFails_SendsFriendlyErrorMessage()
    {
        const long chatId = 100L;
        var message = new Message { Chat = new Chat { Id = chatId } };

        _botConnectionMock.Setup(x => x.Client()).Returns(_botClientMock.Object);
        _botClientMock
            .Setup(x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());

        var sut = new TestSafeResolver(_botConnectionMock.Object) { ShouldThrowCore = true };

        await sut.Execute(message);

        _botClientMock.Verify(
            x => x.SendRequest(
                It.Is<SendMessageRequest>(r => r.ChatId == chatId && r.Text == StandardFriendlyErrorMessage),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenCoreAndFriendlyMessageFail_DoesNotThrow()
    {
        var message = new Message { Chat = new Chat { Id = 200L } };

        _botConnectionMock.Setup(x => x.Client()).Returns(_botClientMock.Object);
        _botClientMock
            .Setup(x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("telegram unavailable"));

        var sut = new TestSafeResolver(_botConnectionMock.Object) { ShouldThrowCore = true };

        var exception = await Record.ExceptionAsync(() => sut.Execute(message));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Execute_WhenCoreSucceeds_DoesNotSendFriendlyErrorMessage()
    {
        var message = new Message { Chat = new Chat { Id = 300L } };

        _botConnectionMock.Setup(x => x.Client()).Returns(_botClientMock.Object);

        var sut = new TestSafeResolver(_botConnectionMock.Object);

        await sut.Execute(message);

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private sealed class TestSafeResolver : SafeTelegramMessageResolver
    {
        public bool ShouldThrowCore { get; init; }

        public TestSafeResolver(ITelegramBotConnection botConnection)
            : base(botConnection, NullLogger.Instance)
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
