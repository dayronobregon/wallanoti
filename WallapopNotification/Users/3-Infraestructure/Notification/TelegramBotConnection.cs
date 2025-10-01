using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace WallapopNotification.Users._3_Infraestructure.Notification;

public sealed class TelegramBotConnection
{
    private static TelegramBotClient? BotClient { get; set; }
    private readonly string _token;

    public TelegramBotConnection(IConfiguration configuration)
    {
        _token = configuration.GetValue<string>("TelegramBotToken");
    }

    public TelegramBotClient Client()
    {
        var options = new TelegramBotClientOptions(_token) { RetryThreshold = 120, RetryCount = 2 };
        return BotClient ??= new TelegramBotClient(options);
    }
}