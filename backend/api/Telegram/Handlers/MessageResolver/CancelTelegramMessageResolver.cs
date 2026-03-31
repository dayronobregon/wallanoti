using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class CancelTelegramMessageResolver : SafeTelegramMessageResolver
{
    public const string Command = "/cancel";
    
    private readonly ITelegramConversationRepository _conversationRepository;

    public CancelTelegramMessageResolver(
        IPushNotificationSender pushNotificationSender,
        ITelegramConversationRepository conversationRepository,
        ILogger<CancelTelegramMessageResolver> logger)
        : base(pushNotificationSender, logger)
    {
        _conversationRepository = conversationRepository;
    }

    protected override async Task ExecuteCore(Message message)
    {
        var chatId = message.Chat.Id;
        var state = await _conversationRepository.GetStateAsync(chatId);

        if (state == ConversationState.Idle)
        {
            await PushNotificationSender.Notify(chatId,
                "No tienes ninguna operación en curso.");
            return;
        }

        await _conversationRepository.ClearAsync(chatId);

        await PushNotificationSender.Notify(chatId,
            "Operación cancelada ✅");
    }
}
