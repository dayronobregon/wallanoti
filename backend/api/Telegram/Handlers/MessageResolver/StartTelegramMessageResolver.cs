using MediatR;
using Telegram.Bot.Types;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Users.Application.CreateUser;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class StartTelegramMessageResolver : SafeTelegramMessageResolver
{
    public const string Command = "/start";
    private readonly IServiceScopeFactory _scopeFactory;

    public StartTelegramMessageResolver(IServiceScopeFactory scopeFactory,
        IPushNotificationSender pushNotificationSender, ILogger<StartTelegramMessageResolver> logger
    ) : base(pushNotificationSender, logger)
    {
        _scopeFactory = scopeFactory;
    }


    protected override async Task ExecuteCore(Message message)
    {
        await PushNotificationSender.Notify(message.Chat.Id,
            "Hola bienvenido a Wallapop Notification. Estamos preparando todo...solo serán unos segundos");

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new CreateUserCommand(message.Chat.Id, message.Chat.Username));

        await PushNotificationSender.Notify(message.Chat.Id,
            "Ya puedes crear alertas con el comando /alert. Simplemente escribe /alert y te pediré la URL de búsqueda de Wallapop 🔗");
    }
}