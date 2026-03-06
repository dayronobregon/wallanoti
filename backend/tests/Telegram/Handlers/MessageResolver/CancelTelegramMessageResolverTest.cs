using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class CancelTelegramMessageResolverTest
{
    private readonly Mock<ITelegramBotClient> _botClientMock = new();
    private readonly Mock<ITelegramBotConnection> _botConnectionMock = new();
    private readonly Mock<ITelegramConversationRepository> _conversationRepoMock = new();

    private readonly CancelTelegramMessageResolver _sut;

    public CancelTelegramMessageResolverTest()
    {
        _botConnectionMock.Setup(x => x.Client()).Returns(_botClientMock.Object);
        _botClientMock
            .Setup(x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());

        _sut = new CancelTelegramMessageResolver(_botConnectionMock.Object, _conversationRepoMock.Object);
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
        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
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
        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
