using MediatR;
using Telegram.Bot.Types;
using Wallanoti.Src.Alerts.Application.DeleteAlert;

namespace Wallanoti.Api.Telegram.Handlers;

public sealed class OnUpdateHandlerFactory
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OnUpdateHandlerFactory> _logger;

    public OnUpdateHandlerFactory(IServiceScopeFactory scopeFactory, ILogger<OnUpdateHandlerFactory> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(Update update)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var callbackQueryData = update.CallbackQuery?.Data;

        if (callbackQueryData is null)
        {
            _logger.LogInformation("Telegram update skipped because callback data is null. updateId={UpdateId}", update.Id);
            return;
        }

        _logger.LogInformation(
            "Telegram update routing started. updateId={UpdateId}, callbackData={CallbackData}",
            update.Id,
            callbackQueryData);

        if (callbackQueryData.StartsWith("delete:"))
        {
            var parts = callbackQueryData.Split(':');
            if (parts.Length != 2 || !Guid.TryParse(parts[1], out var alertId))
            {
                _logger.LogWarning(
                    "Telegram update has invalid delete callback data. updateId={UpdateId}, callbackData={CallbackData}",
                    update.Id,
                    callbackQueryData);
                return;
            }

            _logger.LogInformation(
                "Telegram update selected delete alert command. updateId={UpdateId}, alertId={AlertId}",
                update.Id,
                alertId);

            await mediator.Send(new DeleteAlertCommandRequest(alertId));
            return;
        }

        _logger.LogInformation(
            "Telegram update callback data has no matching route. updateId={UpdateId}, callbackData={CallbackData}",
            update.Id,
            callbackQueryData);
    }
}
