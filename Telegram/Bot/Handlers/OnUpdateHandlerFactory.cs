using MediatR;
using Telegram.Bot.Types;
using WallapopNotification.Alerts._2_Application.DeleteAlert;

namespace Telegram.Bot.Handlers;

public sealed class OnUpdateHandlerFactory
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OnUpdateHandlerFactory(IServiceScopeFactory scopeFactory)
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