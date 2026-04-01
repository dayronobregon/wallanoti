using MediatR;
using Telegram.Bot.Types;
using Wallanoti.Src.Alerts.Application.GetByUser;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class ListTelegramMessageResolver : SafeTelegramMessageResolver
{
    public const string Command = "/list";

    private readonly IServiceScopeFactory _scopeFactory;

    public ListTelegramMessageResolver(IServiceScopeFactory scopeFactory,
        IPushNotificationSender pushNotificationSender, ILogger<ListTelegramMessageResolver> logger) : base(pushNotificationSender, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteCore(Message message)
    {
        var userId = message.Chat.Id;

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var alerts = await mediator.Send(new GetAlertsByUserIdQuery(userId));

        if (alerts.Count == 0)
        {
            await PushNotificationSender.Notify(userId, "No tienes alertas creadas.");
            return;
        }

        foreach (var alert in alerts)
        {
            await PushNotificationSender.Notify(userId,
                alert.Name,
                new PushMessageOptions(
                    ProtectContent: true,
                    ActionButtons:
                    [
                        new PushActionButton("Editar (Próximamente)", $"edit:{alert.Id}"),
                        new PushActionButton("Eliminar", $"delete:{alert.Id}")
                    ]));
        }
    }
}