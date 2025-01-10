using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using WallapopNotification.User._2_Application.CreateUser;
using WallapopNotification.User._3_Infraestructure.Notification;

namespace Client.Bot.Handlers.OnMessage;

public sealed class StartTelegramCommandResolver
{
    public const string Command = "/start";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotConnection _botConnection;

    public StartTelegramCommandResolver(IServiceScopeFactory scopeFactory, TelegramBotConnection botConnection)
    {
        _scopeFactory = scopeFactory;
        _botConnection = botConnection;
    }

    public async Task Execute(Message message)
    {
        await _botConnection.BotClient().SendMessage(message.Chat.Id,
            "Hola bienvenido a Wallapop Notification. Estamos preparando todo...solo serán unos segundos");

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new CreateUserCommand(message.Chat.Id, message.Chat.Username));

        await _botConnection.BotClient().SendMessage(message.Chat.Id,
            "Ya puedes crear alertas con el comando /alert. Ejemplo: /alert, Televisor LG 55 pulgadas, https://es.wallapop.com/item/televisor-lg-55-pulgadas-123456789");
    }
}