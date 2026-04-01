using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Users.Application.CreateUser;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class StartTelegramMessageResolverTest
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IPushNotificationSender> _pushNotificationSenderMock = new();

    [Fact]
    public async Task Execute_SendsWelcomeCreatesUserAndSendsHelpMessage()
    {
        const long chatId = 15L;
        const string username = "dayron";
        var message = new Message { Chat = new Chat { Id = chatId, Username = username } };

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
            .Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _pushNotificationSenderMock
            .Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = new StartTelegramMessageResolver(_scopeFactoryMock.Object, _pushNotificationSenderMock.Object, NullLogger<StartTelegramMessageResolver>.Instance);

        await sut.Execute(message);

        _pushNotificationSenderMock.Verify(
            x => x.Notify(chatId,
                "Hola bienvenido a Wallapop Notification. Estamos preparando todo...solo serán unos segundos"),
            Times.Once);

        _mediatorMock.Verify(
            x => x.Send(
                It.Is<CreateUserCommand>(cmd => cmd.Id == chatId && cmd.UserName == username),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _pushNotificationSenderMock.Verify(
            x => x.Notify(chatId,
                "Ya puedes crear alertas con el comando /alert. Simplemente escribe /alert y te pediré la URL de búsqueda de Wallapop 🔗"),
            Times.Once);
    }
}
