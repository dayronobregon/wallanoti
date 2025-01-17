using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WallapopNotification.Alert._2_Application.ListAlerts;
using WallapopNotification.User._3_Infraestructure.Notification;

namespace Telegram.Bot.Handlers.MessageResolver;

public sealed class ListTelegramCommandResolver
{
    public const string Command = "/list";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotConnection _botConnection;

    public ListTelegramCommandResolver(IServiceScopeFactory scopeFactory, TelegramBotConnection botConnection)
    {
        _scopeFactory = scopeFactory;
        _botConnection = botConnection;
    }

    public async Task Execute(Message message)
    {
        var userId = message.Chat.Id;

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var alerts = await mediator.Send(new GetAlertsByUserIdQuery(userId));

        if (alerts.Count == 0)
        {
            await _botConnection.Client().SendMessage(userId, "No tienes alertas creadas.");
            return;
        }

        foreach (var alert in alerts)
        {
            await _botConnection.Client().SendMessage(userId,
                alert.AlertName,
                protectContent: true,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Editar (Próximamente)", $"edit:{alert.Id}"),
                    InlineKeyboardButton.WithCallbackData("Eliminar", $"delete:{alert.Id}")));
        }
    }
}