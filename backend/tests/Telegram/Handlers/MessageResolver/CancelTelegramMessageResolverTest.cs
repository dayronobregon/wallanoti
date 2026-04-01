using Moq;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging.Abstractions;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class CancelTelegramMessageResolverTest
{
    private readonly Mock<IPushNotificationSender> _pushNotificationSenderMock = new();
    private readonly Mock<ITelegramConversationRepository> _conversationRepoMock = new();

    private readonly CancelTelegramMessageResolver _sut;

    public CancelTelegramMessageResolverTest()
    {
        _pushNotificationSenderMock
            .Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _sut = new CancelTelegramMessageResolver(
            _pushNotificationSenderMock.Object,
            _conversationRepoMock.Object,
            NullLogger<CancelTelegramMessageResolver>.Instance);
    }

    [Fact]
    public async Task Execute_WhenStateIsIdle_SendsNoOperationMessageAndDoesNotClear()
    {
        const long chatId = 123L;
        var message = new Message { Chat = new Chat { Id = chatId }, Text = "/cancel" };

        _conversationRepoMock
            .Setup(x => x.GetStateAsync(chatId))
            .ReturnsAsync(ConversationState.Idle);

        await _sut.Execute(message);

        _conversationRepoMock.Verify(x => x.ClearAsync(It.IsAny<long>()), Times.Never);
        _pushNotificationSenderMock.Verify(
            x => x.Notify(chatId, "No tienes ninguna operación en curso."),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenStateIsAwaitingUrl_ClearsStateAndSendsConfirmation()
    {
        const long chatId = 123L;
        var message = new Message { Chat = new Chat { Id = chatId }, Text = "/cancel" };

        _conversationRepoMock
            .Setup(x => x.GetStateAsync(chatId))
            .ReturnsAsync(ConversationState.AwaitingUrl);
        _conversationRepoMock
            .Setup(x => x.ClearAsync(chatId))
            .Returns(Task.CompletedTask);

        await _sut.Execute(message);

        _conversationRepoMock.Verify(x => x.ClearAsync(chatId), Times.Once);
        _pushNotificationSenderMock.Verify(
            x => x.Notify(chatId, "Operación cancelada ✅"),
            Times.Once);
    }
}
