using MediatR;
using Telegram.Bot.Types;
using WallapopNotification.Users._2_Application.CreateUser;
using WallapopNotification.Users._3_Infraestructure.Notification;

namespace Telegram.Bot.Handlers.MessageResolver;

public sealed class StartTelegramMessageResolver : IMessageResolver
{
    public const string Command = "/start";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotConnection _botConnection;

    public StartTelegramMessageResolver(IServiceScopeFactory scopeFactory, TelegramBotConnection botConnection)
    {
        _scopeFactory = scopeFactory;
        _botConnection = botConnection;
    }


    public async Task Execute(Message message)
    {
        await _botConnection.Client().SendMessage(message.Chat.Id,
            "Hola bienvenido a Wallapop Notification. Estamos preparando todo...solo serán unos segundos");

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new CreateUserCommand(message.Chat.Id, message.Chat.Username));

        await _botConnection.Client().SendMessage(message.Chat.Id,
            "Ya puedes crear alertas con el comando /alert. Ejemplo: /alert, Televisor LG 55 pulgadas, https://es.wallapop.com/item/televisor-lg-55-pulgadas-123456789");
    }
}