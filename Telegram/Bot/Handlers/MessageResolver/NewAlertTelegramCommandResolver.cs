using MediatR;
using Telegram.Bot.Types;
using WallapopNotification.Alert._1_Domain.Models;
using WallapopNotification.Alert._2_Application.CreateAlert;
using WallapopNotification.User._3_Infraestructure.Notification;

namespace Telegram.Bot.Handlers.MessageResolver;

public sealed class NewAlertTelegramCommandResolver
{
    public const string Command = "/alert";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotConnection _botConnection;

    public NewAlertTelegramCommandResolver(IServiceScopeFactory scopeFactory, TelegramBotConnection botConnection)
    {
        _scopeFactory = scopeFactory;
        _botConnection = botConnection;
    }

    public async Task Execute(Message message)
    {
        await _botConnection.Client().SendMessage(message.Chat.Id,
            "Estamos creando la alerta");

        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var @params = message.Text?.Split(",").ToList();

        if (@params is not { Count: 3 })
        {
            await _botConnection.Client().SendMessage(message.Chat.Id,
                "El comando /alert debe tener la siguiente estructura: /alert, nombrealerta, urlWallapop");
            return;
        }


        var alertName = @params[1];
        var alertUrl = @params[2];

        if (AlertEntity.ValidUrl(alertUrl) == false)
        {
            await _botConnection.Client().SendMessage(message.Chat.Id,
                "No es una url valida. Copia la url de búsqueda que has hecho en Wallapop");
            return;
        }

        await mediator.Send(new CreateAlertCommand(message.Chat.Id, alertName, alertUrl));

        await _botConnection.Client().SendMessage(message.Chat.Id,
            $"Alerta {alertName} creada");
    }
}