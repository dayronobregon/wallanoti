namespace Wallanoti.Api.Telegram.Conversation;

public interface ITelegramConversationRepository
{
    Task<ConversationState> GetStateAsync(long chatId);
    Task SetStateAsync(long chatId, ConversationState state);
    Task ClearAsync(long chatId);
}
