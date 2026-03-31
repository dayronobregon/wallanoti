using Moq;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging.Abstractions;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class NewAlertTelegramMessageResolverTest
{
    private const string StandardFriendlyErrorMessage = "Ha ocurrido un error. Será notificado al administrador.";

    private readonly Mock<IPushNotificationSender> _pushNotificationSenderMock = new();
    private readonly Mock<ITelegramConversationRepository> _conversationRepoMock = new();

    private readonly NewAlertTelegramMessageResolver _sut;

    public NewAlertTelegramMessageResolverTest()
    {
        _pushNotificationSenderMock
            .Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _conversationRepoMock
            .Setup(x => x.SetStateAsync(It.IsAny<long>(), It.IsAny<ConversationState>()))
            .Returns(Task.CompletedTask);

        _sut = new NewAlertTelegramMessageResolver(
            _pushNotificationSenderMock.Object,
            _conversationRepoMock.Object,
            NullLogger<NewAlertTelegramMessageResolver>.Instance);
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

        _pushNotificationSenderMock.Verify(
            x => x.Notify(chatId, "Envíame la URL de búsqueda de Wallapop 🔗"),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenSetStateFails_SendsFriendlyErrorMessage()
    {
        const long chatId = 321L;
        var message = new Message { Chat = new Chat { Id = chatId }, Text = "/alert" };

        _conversationRepoMock
            .Setup(x => x.SetStateAsync(chatId, ConversationState.AwaitingUrl))
            .ThrowsAsync(new InvalidOperationException("redis down"));

        await _sut.Execute(message);

        _pushNotificationSenderMock.Verify(
            x => x.Notify(chatId, StandardFriendlyErrorMessage),
            Times.Once);
    }
}
