using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;
using Wallanoti.Src.Users.Application.CreateUser;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

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
            "Ya puedes crear alertas con el comando /alert. Simplemente escribe /alert y te pediré la URL de búsqueda de Wallapop 🔗");
    }
}
