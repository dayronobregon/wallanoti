using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Alerts.Application.CreateAlert;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class AlertUrlTelegramMessageResolverTest
{
    private const string StandardFriendlyErrorMessage = "Ha ocurrido un error. Será notificado al administrador.";

    private readonly Mock<ITelegramBotClient> _botClientMock = new();
    private readonly Mock<ITelegramBotConnection> _botConnectionMock = new();
    private readonly Mock<ITelegramConversationRepository> _conversationRepoMock = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();

    private readonly AlertUrlTelegramMessageResolver _sut;

    public AlertUrlTelegramMessageResolverTest()
    {
        _botConnectionMock.Setup(x => x.Client()).Returns(_botClientMock.Object);
        _botClientMock
            .Setup(x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(scopeMock.Object);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateAlertCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        _sut = new AlertUrlTelegramMessageResolver(
            _scopeFactoryMock.Object,
            _botConnectionMock.Object,
            _conversationRepoMock.Object,
            NullLogger<AlertUrlTelegramMessageResolver>.Instance);
    }

    [Fact]
    public async Task Execute_WhenUrlIsNotWallapop_SendsErrorAndDoesNotChangeState()
    {
        const long chatId = 123L;
        var message = new Message { Chat = new Chat { Id = chatId }, Text = "https://not-wallapop.com/search?keywords=iphone" };

        await _sut.Execute(message);

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _conversationRepoMock.Verify(x => x.ClearAsync(It.IsAny<long>()), Times.Never);
        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateAlertCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WhenUrlHasNoKeywords_SendsErrorAndDoesNotChangeState()
    {
        const long chatId = 123L;
        var message = new Message { Chat = new Chat { Id = chatId }, Text = "https://es.wallapop.com/search?category_id=24200" };

        await _sut.Execute(message);

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _conversationRepoMock.Verify(x => x.ClearAsync(It.IsAny<long>()), Times.Never);
        _mediatorMock.Verify(x => x.Send(It.IsAny<CreateAlertCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_WithValidUrl_ExtractsKeywordsCreatesAlertAndClearsState()
    {
        const long chatId = 123L;
        const string url = "https://es.wallapop.com/search?keywords=iphone+14&category_id=24200";
        var message = new Message { Chat = new Chat { Id = chatId }, Text = url };

        _conversationRepoMock
            .Setup(x => x.ClearAsync(chatId))
            .Returns(Task.CompletedTask);

        await _sut.Execute(message);

        _mediatorMock.Verify(
            x => x.Send(
                It.Is<CreateAlertCommand>(cmd => cmd.AlertName == "iphone 14" && cmd.UserId == chatId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _conversationRepoMock.Verify(x => x.ClearAsync(chatId), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenCreateAlertFails_SendsFriendlyErrorAndKeepsState()
    {
        const long chatId = 777L;
        const string url = "https://es.wallapop.com/search?keywords=nintendo+switch";
        var message = new Message { Chat = new Chat { Id = chatId }, Text = url };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<CreateAlertCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db timeout"));

        await _sut.Execute(message);

        _conversationRepoMock.Verify(x => x.ClearAsync(It.IsAny<long>()), Times.Never);
        _botClientMock.Verify(
            x => x.SendRequest(
                It.Is<SendMessageRequest>(r => r.ChatId == chatId && r.Text == StandardFriendlyErrorMessage),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
