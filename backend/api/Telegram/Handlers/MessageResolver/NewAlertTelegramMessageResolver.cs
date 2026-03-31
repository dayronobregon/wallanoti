using Telegram.Bot.Types;
using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class NewAlertTelegramMessageResolver : SafeTelegramMessageResolver
{
    public const string Command = "/alert";
    
    private readonly ITelegramConversationRepository _conversationRepository;

    public NewAlertTelegramMessageResolver(
        IPushNotificationSender pushNotificationSender,
        ITelegramConversationRepository conversationRepository,
        ILogger<NewAlertTelegramMessageResolver> logger)
        : base(pushNotificationSender, logger)
    {
        _conversationRepository = conversationRepository;
    }

    protected override async Task ExecuteCore(Message message)
    {
        var chatId = message.Chat.Id;

        await _conversationRepository.SetStateAsync(chatId, ConversationState.AwaitingUrl);

        await PushNotificationSender.Notify(chatId,
            "Envíame la URL de búsqueda de Wallapop 🔗");
    }
}
