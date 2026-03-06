using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace Wallanoti.Src.Notifications.Infrastructure.Telegram;

public sealed class TelegramBotConnection : ITelegramBotConnection
{
    private static TelegramBotClient? BotClient { get; set; }
    private readonly string _token;

    public TelegramBotConnection(IConfiguration configuration)
    {
        _token = configuration.GetValue<string>("TelegramBotToken");
    }

    public ITelegramBotClient Client()
    {
        var options = new TelegramBotClientOptions(_token) { RetryThreshold = 120, RetryCount = 2 };
        return BotClient ??= new TelegramBotClient(options);
    }
}