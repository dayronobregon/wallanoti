using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class NewAlertTelegramMessageResolverTest
{
    private readonly Mock<ITelegramBotClient> _botClientMock = new();
    private readonly Mock<ITelegramBotConnection> _botConnectionMock = new();
    private readonly Mock<ITelegramConversationRepository> _conversationRepoMock = new();

    private readonly NewAlertTelegramMessageResolver _sut;

    public NewAlertTelegramMessageResolverTest()
    {
        _botConnectionMock.Setup(x => x.Client()).Returns(_botClientMock.Object);
        _botClientMock
            .Setup(x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());
        _conversationRepoMock
            .Setup(x => x.SetStateAsync(It.IsAny<long>(), It.IsAny<ConversationState>()))
            .Returns(Task.CompletedTask);

        _sut = new NewAlertTelegramMessageResolver(_botConnectionMock.Object, _conversationRepoMock.Object);
    }

    [Fact]
    public async Task Execute_SetsStateToAwaitingUrlAndSendsPrompt()
    {
        const long chatId = 123L;
        var message = new Message { Chat = new Chat { Id = chatId }, Text = "/alert" };

        await _sut.Execute(message);

        _conversationRepoMock.Verify(
            x => x.SetStateAsync(chatId, ConversationState.AwaitingUrl),
            Times.Once);

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
