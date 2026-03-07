using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Wallanoti.Api.Telegram.Conversation;

namespace Wallanoti.Tests.Telegram.Conversation;

public class RedisTelegramConversationRepositoryTest
{
    private readonly IDistributedCache _cache;
    private readonly RedisTelegramConversationRepository _sut;

    public RedisTelegramConversationRepositoryTest()
    {
        _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _sut = new RedisTelegramConversationRepository(_cache);
    }

    [Fact]
    public async Task GetStateAsync_WhenKeyNotInCache_ReturnsIdle()
    {
        var state = await _sut.GetStateAsync(chatId: 42L);

        Assert.Equal(ConversationState.Idle, state);
    }

    [Fact]
    public async Task SetStateAsync_StoresStateInCache()
    {
        await _sut.SetStateAsync(chatId: 42L, ConversationState.AwaitingUrl);

        var raw = _cache.GetString("telegram:conv:42");
        Assert.Equal(nameof(ConversationState.AwaitingUrl), raw);
    }

    [Fact]
    public async Task GetStateAsync_WhenStateIsAwaitingUrl_ReturnsAwaitingUrl()
    {
        await _sut.SetStateAsync(chatId: 42L, ConversationState.AwaitingUrl);

        var state = await _sut.GetStateAsync(chatId: 42L);

        Assert.Equal(ConversationState.AwaitingUrl, state);
    }

    [Fact]
    public async Task ClearAsync_RemovesStateFromCache()
    {
        await _sut.SetStateAsync(chatId: 42L, ConversationState.AwaitingUrl);

        await _sut.ClearAsync(chatId: 42L);

        var state = await _sut.GetStateAsync(chatId: 42L);
        Assert.Equal(ConversationState.Idle, state);
    }
}
