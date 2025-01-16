using MediatR;
using Telegram.Bot.Types;
using WallapopNotification.Alert._2_Application.DeleteAlert;

namespace Client.Bot.Handlers;

public sealed class OnUpdateHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OnUpdateHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(Update update)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var callbackQueryData = update.CallbackQuery?.Data;

        if (callbackQueryData is null)
        {
            return;
        }

        if (callbackQueryData.StartsWith("delete:"))
        {
            var alertId = Guid.Parse(callbackQueryData.Split(":")[1]);

            await mediator.Send(new DeleteAlertCommandRequest(alertId));
        }
    }
}