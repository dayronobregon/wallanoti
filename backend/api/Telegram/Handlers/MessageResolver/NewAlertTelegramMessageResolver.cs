using Telegram.Bot;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class NewAlertTelegramMessageResolver : IMessageResolver
{
    public const string Command = "/alert";

    private readonly ITelegramBotConnection _botConnection;
    private readonly ITelegramConversationRepository _conversationRepository;

    public NewAlertTelegramMessageResolver(
        ITelegramBotConnection botConnection,
        ITelegramConversationRepository conversationRepository)
    {
        _botConnection = botConnection;
        _conversationRepository = conversationRepository;
    }

    public async Task Execute(Message message)
    {
        var chatId = message.Chat.Id;

        await _conversationRepository.SetStateAsync(chatId, ConversationState.AwaitingUrl);

        await _botConnection.Client().SendMessage(chatId,
            "Envíame la URL de búsqueda de Wallapop 🔗");
    }
}
