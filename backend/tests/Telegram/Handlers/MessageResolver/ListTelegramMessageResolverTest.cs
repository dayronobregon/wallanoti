using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Alerts.Application.GetByUser;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class ListTelegramMessageResolverTest
{
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IPushNotificationSender> _pushNotificationSenderMock = new();

    [Fact]
    public async Task Execute_WhenUserHasNoAlerts_SendsEmptyMessage()
    {
        const long chatId = 123L;
        var message = new Message { Chat = new Chat { Id = chatId } };

        ConfigureMediator(Array.Empty<GetAlertsByUserIdResponse>());
        _pushNotificationSenderMock
            .Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = new ListTelegramMessageResolver(_scopeFactoryMock.Object, _pushNotificationSenderMock.Object, NullLogger<ListTelegramMessageResolver>.Instance);

        await sut.Execute(message);

        _pushNotificationSenderMock.Verify(x => x.Notify(chatId, "No tienes alertas creadas."), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUserHasAlerts_SendsAlertWithActionButtons()
    {
        const long chatId = 123L;
        var message = new Message { Chat = new Chat { Id = chatId } };
        var alerts = new[] { new GetAlertsByUserIdResponse(Guid.NewGuid(), "iPhone", "https://es.wallapop.com/search?keywords=iphone", true) };

        ConfigureMediator(alerts);
        _pushNotificationSenderMock
            .Setup(x => x.Notify(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<PushMessageOptions?>()))
            .Returns(Task.CompletedTask);

        var sut = new ListTelegramMessageResolver(_scopeFactoryMock.Object, _pushNotificationSenderMock.Object, NullLogger<ListTelegramMessageResolver>.Instance);

        await sut.Execute(message);

        _pushNotificationSenderMock.Verify(
            x => x.Notify(
                chatId,
                "iPhone",
                It.Is<PushMessageOptions>(options =>
                    options.ProtectContent &&
                    options.ActionButtons.Count == 2 &&
                    options.ActionButtons[0].Text == "Editar (Próximamente)" &&
                    options.ActionButtons[0].CallbackData == $"edit:{alerts[0].Id}" &&
                    options.ActionButtons[1].Text == "Eliminar" &&
                    options.ActionButtons[1].CallbackData == $"delete:{alerts[0].Id}")),
            Times.Once);
    }

    private void ConfigureMediator(IReadOnlyCollection<GetAlertsByUserIdResponse> alerts)
    {
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
            .Setup(x => x.Send(It.IsAny<GetAlertsByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts.ToList());
    }
}
