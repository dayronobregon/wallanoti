using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Wallanoti.Src.Alerts.Application.GetByUser;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class ListTelegramMessageResolver : IMessageResolver
{
    public const string Command = "/list";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotConnection _botConnection;

    public ListTelegramMessageResolver(IServiceScopeFactory scopeFactory, TelegramBotConnection botConnection)
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
                alert.Name,
                protectContent: true,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Editar (Próximamente)", $"edit:{alert.Id}"),
                    InlineKeyboardButton.WithCallbackData("Eliminar", $"delete:{alert.Id}")));
        }
    }
}