using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace WallapopNotification.User._3_Infraestructure.Notification;

public sealed class TelegramBotConnection
{
    private static TelegramBotClient? _botClient { get; set; }
    private readonly string _token;

    public TelegramBotConnection(IConfiguration configuration)
    {
        _token = configuration.GetValue<string>("Telegram:BotToken");
    }

    public TelegramBotClient BotClient()
    {
        var options = new TelegramBotClientOptions(_token) { RetryThreshold = 120, RetryCount = 2 };
        return _botClient ??= new TelegramBotClient(options);
    }
}