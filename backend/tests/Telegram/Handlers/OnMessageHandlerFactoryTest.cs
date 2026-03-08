using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Tests.Telegram.Handlers;

public class OnMessageHandlerFactoryTest
{
    private readonly Mock<ITelegramBotConnection> _botConnectionMock = new();
    private readonly Mock<ITelegramConversationRepository> _conversationRepoMock = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly Mock<IPushNotificationSender> _pushNotificationSenderMock = new();

    private readonly StartTelegramMessageResolver _startResolver;
    private readonly NewAlertTelegramMessageResolver _newAlertResolver;
    private readonly ListTelegramMessageResolver _listResolver;
    private readonly AlertUrlTelegramMessageResolver _alertUrlResolver;
    private readonly CancelTelegramMessageResolver _cancelResolver;
    private readonly UnknownTelegramMessageResolver _unknownResolver;

    private readonly OnMessageHandlerFactory _sut;

    public OnMessageHandlerFactoryTest()
    {
        _startResolver = new StartTelegramMessageResolver(_scopeFactoryMock.Object, _botConnectionMock.Object);
        _newAlertResolver =
            new NewAlertTelegramMessageResolver(_botConnectionMock.Object, _conversationRepoMock.Object);
        _listResolver = new ListTelegramMessageResolver(_scopeFactoryMock.Object, _botConnectionMock.Object);
        _alertUrlResolver = new AlertUrlTelegramMessageResolver(_scopeFactoryMock.Object, _botConnectionMock.Object,
            _conversationRepoMock.Object);
        _cancelResolver = new CancelTelegramMessageResolver(_botConnectionMock.Object, _conversationRepoMock.Object);
        _unknownResolver = new UnknownTelegramMessageResolver(_pushNotificationSenderMock.Object);

        _sut = new OnMessageHandlerFactory(
            _startResolver,
            _newAlertResolver,
            _listResolver,
            _alertUrlResolver,
            _cancelResolver,
            _unknownResolver,
            _conversationRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageIsCancel_ReturnsCancelResolver()
    {
        var resolver = await _sut.HandleAsync(chatId: 1L, messageText: "/cancel");

        Assert.Same(_cancelResolver, resolver);
    }

    [Fact]
    public async Task HandleAsync_WhenFreeTextAndStateIsAwaitingUrl_ReturnsAlertUrlResolver()
    {
        _conversationRepoMock
            .Setup(x => x.GetStateAsync(1L))
            .ReturnsAsync(ConversationState.AwaitingUrl);

        var resolver =
            await _sut.HandleAsync(chatId: 1L, messageText: "https://es.wallapop.com/search?keywords=iphone");

        Assert.Same(_alertUrlResolver, resolver);
    }

    [Fact]
    public async Task HandleAsync_WhenFreeTextAndStateIsIdle_ReturnsUnknownResolver()
    {
        _conversationRepoMock
            .Setup(x => x.GetStateAsync(1L))
            .ReturnsAsync(ConversationState.Idle);

        var resolver = await _sut.HandleAsync(chatId: 1L, messageText: "hello");

        Assert.Same(_unknownResolver, resolver);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsAlert_ReturnsNewAlertResolver()
    {
        var resolver = await _sut.HandleAsync(chatId: 1L, messageText: "/alert");

        Assert.Same(_newAlertResolver, resolver);
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsList_ReturnsListResolver()
    {
        var resolver = await _sut.HandleAsync(chatId: 1L, messageText: "/list");

        Assert.Same(_listResolver, resolver);
    }

    [Fact]
    public async Task HandleAsync_WhenStartCommand_ReturnsStartResolver()
    {
        var resolver = await _sut.HandleAsync(chatId: 1L, messageText: "/start");

        Assert.Same(_startResolver, resolver);
    }

    [Fact]
    public async Task HandleAsync_WhenUnknownCommand_ReturnsUnknownResolver()
    {
        var resolver = await _sut.HandleAsync(chatId: 1L, messageText: "/unknown");

        Assert.Same(_unknownResolver, resolver);
    }
}