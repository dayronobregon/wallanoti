using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Src.Alerts.Application.CreateAlert;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class AlertUrlTelegramMessageResolver : IMessageResolver
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITelegramBotConnection _botConnection;
    private readonly ITelegramConversationRepository _conversationRepository;

    public AlertUrlTelegramMessageResolver(
        IServiceScopeFactory scopeFactory,
        ITelegramBotConnection botConnection,
        ITelegramConversationRepository conversationRepository)
    {
        _scopeFactory = scopeFactory;
        _botConnection = botConnection;
        _conversationRepository = conversationRepository;
    }

    public async Task Execute(Message message)
    {
        var chatId = message.Chat.Id;
        var text = message.Text ?? string.Empty;

        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            await _botConnection.Client().SendMessage(chatId,
                "La URL no es válida. Por favor, copia la URL directamente desde Wallapop.");
            return;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, System.StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.Host, "es.wallapop.com", System.StringComparison.OrdinalIgnoreCase))
        {
            await _botConnection.Client().SendMessage(chatId,
                "La URL debe ser de Wallapop (es.wallapop.com). Por favor, envíame la URL de búsqueda correcta.");
            return;
        }

        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var rawKeywords = queryParams["keywords"];

        if (string.IsNullOrWhiteSpace(rawKeywords))
        {
            await _botConnection.Client().SendMessage(chatId,
                "La URL no contiene el parámetro de búsqueda (keywords). Asegúrate de buscar algo en Wallapop y copiar la URL de los resultados.");
            return;
        }

        var alertName = rawKeywords;

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new CreateAlertCommand(chatId, alertName, text));

        await _conversationRepository.ClearAsync(chatId);

        await _botConnection.Client().SendMessage(chatId,
            $"Alerta \"{alertName}\" creada ✅");
    }
}
