using Microsoft.Extensions.Caching.Distributed;

namespace Wallanoti.Api.Telegram.Conversation;

public sealed class RedisTelegramConversationRepository : ITelegramConversationRepository
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    public RedisTelegramConversationRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<ConversationState> GetStateAsync(long chatId)
    {
        var value = await _cache.GetStringAsync(Key(chatId));

        if (value is null)
            return ConversationState.Idle;

        return Enum.TryParse<ConversationState>(value, out var state)
            ? state
            : ConversationState.Idle;
    }

    public Task SetStateAsync(long chatId, ConversationState state)
    {
        return _cache.SetStringAsync(Key(chatId), state.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        });
    }

    public Task ClearAsync(long chatId)
    {
        return _cache.RemoveAsync(Key(chatId));
    }

    private static string Key(long chatId) => $"telegram:conv:{chatId}";
}
