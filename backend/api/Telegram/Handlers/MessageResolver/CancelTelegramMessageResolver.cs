using Telegram.Bot;
using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class CancelTelegramMessageResolver : IMessageResolver
{
    public const string Command = "/cancel";

    private readonly ITelegramBotConnection _botConnection;
    private readonly ITelegramConversationRepository _conversationRepository;

    public CancelTelegramMessageResolver(
        ITelegramBotConnection botConnection,
        ITelegramConversationRepository conversationRepository)
    {
        _botConnection = botConnection;
        _conversationRepository = conversationRepository;
    }

    public async Task Execute(Message message)
    {
        var chatId = message.Chat.Id;
        var state = await _conversationRepository.GetStateAsync(chatId);

        if (state == ConversationState.Idle)
        {
            await _botConnection.Client().SendMessage(chatId,
                "No tienes ninguna operación en curso.");
            return;
        }

        await _conversationRepository.ClearAsync(chatId);

        await _botConnection.Client().SendMessage(chatId,
            "Operación cancelada ✅");
    }
}
